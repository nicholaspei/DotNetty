// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    public class HttpRequestEncoder : HttpObjectEncoder<IHttpRequest>
    {
        const char Slash = '/';
        const char QuestionMark = '?';

        public override bool AcceptOutboundMessage(object msg) => 
            base.AcceptOutboundMessage(msg) && !(msg is IHttpResponse);

        protected internal override void EncodeInitialLine(IByteBuffer buf, IHttpRequest request)
        {
            AsciiString method = request.Method.AsciiName();
            ByteBufferUtil.Copy(method, method.Offset, buf, method.Count);
            buf.WriteByte(HttpConstants.HorizontalSpace);

            // Add / as absolute path if no is present.
            // See http://tools.ietf.org/html/rfc2616#section-5.1.2
            string uri = request.Uri ?? string.Empty;
            if (uri.Length == 0)
            {
                uri += Slash;
            }
            else
            {
                int start = uri.IndexOf("://", StringComparison.Ordinal);
                if (start != -1 && uri[0] != Slash)
                {
                    int startIndex = start + 3;
                    // Correctly handle query params.
                    // See https://github.com/netty/netty/issues/2732
                    int index = uri.IndexOf(QuestionMark, startIndex);
                    if (index == -1)
                    {
                        if (uri.LastIndexOf(Slash) <= startIndex)
                        {
                            uri += Slash;
                        }
                    }
                    else
                    {
                        if (uri.LastIndexOf(Slash, index) <= startIndex)
                        {
                            int len = uri.Length;
                            var sb = new StringBuilder(len + 1);
                            sb.Append(uri, 0, index)
                              .Append(Slash)
                              .Append(uri, index, len - index);
                            uri = sb.ToString();
                        }
                    }
                }
            }

            buf.WriteBytes(Encoding.UTF8.GetBytes(uri));

            buf.WriteByte(HttpConstants.HorizontalSpace);
            request.ProtocolVersion.Encode(buf);
            buf.WriteBytes(CRLF);
        }
    }
}
