﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http.Cors
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class CorsHandler : ChannelDuplexHandler
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<CorsHandler>();

        static readonly AsciiString NullOrigin = new AsciiString("null");

        readonly CorsConfig config;
        IHttpRequest request;

        public CorsHandler(CorsConfig config)
        {
            Contract.Requires(config != null);

            this.config = config;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (this.config.IsCorsSupportEnabled && message is IHttpRequest)
            {
                this.request = (IHttpRequest)message;
                if (IsPreflightRequest(this.request))
                {
                    this.HandlePreflight(context, this.request);
                    return;
                }
                if (this.config.IsShortCircuit && !this.ValidateOrigin())
                {
                    Forbidden(context, this.request);
                    return;
                }
            }

            context.FireChannelRead(message);
        }

        void HandlePreflight(IChannelHandlerContext ctx, IHttpRequest req)
        {
            var response = new DefaultFullHttpResponse(req.ProtocolVersion, HttpResponseStatus.OK, true, true);
            if (this.SetOrigin(response))
            {
                this.SetAllowMethods(response);
                this.SetAllowHeaders(response);
                this.SetAllowCredentials(response);
                this.SetMaxAge(response);
                this.SetPreflightHeaders(response);
            }

            ReferenceCountUtil.Release(req);
            Respond(ctx, req, response);
        }

        void SetPreflightHeaders(IHttpResponse response) => response.Headers.Add(this.config.PreflightResponseHeaders);

        bool SetOrigin(IHttpResponse response)
        {
            ICharSequence origin = this.request.Headers.Get(HttpHeaderNames.Origin);
            if (origin == null)
            {
                return false;
            }
            if (NullOrigin.Equals(origin) && this.config.IsNullOriginAllowed)
            {
                SetNullOrigin(response);
                return true;
            }

            if (this.config.IsAnyOriginSupported)
            {
                if (this.config.IsCredentialsAllowed)
                {
                    this.EchoRequestOrigin(response);
                    SetVaryHeader(response);
                }
                else
                {
                    SetAnyOrigin(response);
                }
                return true;
            }

            if (this.config.Origins.Contains(origin))
            {
                SetOrigin(response, origin);
                SetVaryHeader(response);
                return true;
            }

            if (Logger.DebugEnabled)
            {
                Logger.Debug("Request origin [{}]] was not among the configured origins [{}]", origin, this.config.Origins);
            }

            return false;
        }

        bool ValidateOrigin()
        {
            if (this.config.IsAnyOriginSupported)
            {
                return true;
            }

            ICharSequence origin = this.request.Headers.Get(HttpHeaderNames.Origin);
            if (origin == null)
            {
                // Not a CORS request so we cannot validate it. It may be a non CORS request.
                return true;
            }

            if (NullOrigin.Equals(origin) && this.config.IsNullOriginAllowed)
            {
                return true;
            }

            return this.config.Origins.Contains(origin);
        }

        void EchoRequestOrigin(IHttpResponse response) => SetOrigin(response, this.request.Headers.Get(HttpHeaderNames.Origin));

        static void SetVaryHeader(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.Vary, HttpHeaderNames.Origin);

        static void SetAnyOrigin(IHttpResponse response) => SetOrigin(response, CorsConfig.AnyOriginValue);

        static void SetNullOrigin(IHttpResponse response) => SetOrigin(response, NullOrigin);

        static void SetOrigin(IHttpResponse response, ICharSequence origin) => response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, origin);

        void SetAllowCredentials(IHttpResponse response)
        {
            if (this.config.IsCredentialsAllowed
                && !response.Headers.Get(HttpHeaderNames.AccessControlAllowOrigin).Equals(CorsConfig.AnyOriginValue))
            {
                response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, new AsciiString("true"));
            }
        }

        static bool IsPreflightRequest(IHttpRequest request)
        {
            HttpHeaders headers = request.Headers;
            return request.Method.Equals(HttpMethod.Options) 
                && headers.Contains(HttpHeaderNames.Origin) 
                && headers.Contains(HttpHeaderNames.AccessControlRequestMethod);
        }

        void SetExposeHeaders(IHttpResponse response)
        {
            if (this.config.ExposedHeaders.Count > 0)
            {
                response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, this.config.ExposedHeaders);
            }
        }

        void SetAllowMethods(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, this.config.AllowedRequestMethods);

        void SetAllowHeaders(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, this.config.AllowedRequestHeaders);

        void SetMaxAge(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, this.config.MaxAge);

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (this.config.IsCorsSupportEnabled && message is IHttpResponse) {
                var response = (IHttpResponse)message;
                if (this.SetOrigin(response))
                {
                    this.SetAllowCredentials(response);
                    this.SetExposeHeaders(response);
                }
            }

            return context.WriteAndFlushAsync(message);
        }

        static void Forbidden(IChannelHandlerContext ctx, IHttpRequest request)
        {
            ReferenceCountUtil.Release(request);
            Respond(ctx, request, new DefaultFullHttpResponse(request.ProtocolVersion, HttpResponseStatus.Forbidden));
        }

        static void Respond(IChannelHandlerContext ctx, IHttpRequest request, IHttpResponse response)
        {
            bool keepAlive = HttpUtil.IsKeepAlive(request);
            HttpUtil.SetKeepAlive(response, keepAlive);
            ctx.WriteAndFlushAsync(response);

            if (!keepAlive)
            {
                ctx.CloseAsync();
            }
        }
    }
}
