// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public abstract class HttpObjectEncoder<T> : MessageToMessageEncoder<object> 
        where T : IHttpMessage
    {
        protected static readonly byte[] CRLF = { HttpConstants.CarriageReturn, HttpConstants.LineFeed };
        static readonly byte[] ZERO_CRLF = { (byte)'0', HttpConstants.CarriageReturn, HttpConstants.LineFeed };
        static readonly byte[] ZERO_CRLF_CRLF = { (byte)'0', HttpConstants.CarriageReturn, HttpConstants.LineFeed, HttpConstants.CarriageReturn, HttpConstants.LineFeed };
        static readonly IByteBuffer CRLF_BUF = Unpooled.WrappedBuffer(CRLF).Unreleasable();
        static readonly IByteBuffer ZERO_CRLF_CRLF_BUF = Unpooled.WrappedBuffer(ZERO_CRLF_CRLF).Unreleasable();

        const int ST_INIT = 0;
        const int ST_CONTENT_NON_CHUNK = 1;
        const int ST_CONTENT_CHUNK = 2;
        const int ST_CONTENT_ALWAYS_EMPTY = 3;

        int state = ST_INIT;

        protected override void Encode(IChannelHandlerContext context, object message, List<object> output)
        {
            IByteBuffer buf = null;
            if (message is IHttpMessage)
            {
                if (this.state != ST_INIT)
                {
                    throw new EncoderException($"unexpected message type: {StringUtil.SimpleClassName(message)}");
                }

                var m = (T)message;
                buf = context.Allocator.Buffer();
                // Encode the message.
                this.EncodeInitialLine(buf, m);
                this.EncodeHeaders(m.Headers, buf);
                buf.WriteBytes(CRLF);

                this.state = this.IsContentAlwaysEmpty(m) 
                    ? ST_CONTENT_ALWAYS_EMPTY 
                    : HttpUtil.IsTransferEncodingChunked(m) ? ST_CONTENT_CHUNK : ST_CONTENT_NON_CHUNK;
            }

            // Bypass the encoder in case of an empty buffer, so that the following idiom works:
            //
            //     ch.write(Unpooled.EMPTY_BUFFER).addListener(ChannelFutureListener.CLOSE);
            //
            // See https://github.com/netty/netty/issues/2983 for more information.

            var buffer = message as IByteBuffer;
            if (buffer != null && !buffer.IsReadable())
            {
                output.Add(Unpooled.Empty);
                return;
            }

            if (message is IHttpContent || message is IByteBuffer || message is IFileRegion)
            {
                if (this.state == ST_INIT)
                {
                    throw new EncoderException($"unexpected message type: {StringUtil.SimpleClassName(message)}");
                }

                switch (this.state)
                {
                    case ST_CONTENT_NON_CHUNK:
                        long contentLength = ContentLength(message);
                        if (contentLength > 0)
                        {
                            if (buf != null && buf.WritableBytes >= contentLength && message is IHttpContent)
                            {
                                // merge into other buffer for performance reasons
                                buf.WriteBytes(((IHttpContent)message).Content);
                                output.Add(buf);
                            }
                            else
                            {
                                if (buf != null)
                                {
                                    output.Add(buf);
                                }

                                output.Add(EncodeAndRetain(message));
                            }

                            if (message is ILastHttpContent)
                            {
                                this.state = ST_INIT;
                            }
                        }
                        else
                        {
                            output.Add(buf ?? Unpooled.Empty);
                        }
                        break;
                    case ST_CONTENT_ALWAYS_EMPTY:
                        // We allocated a buffer so add it now.
                        // Need to produce some output otherwise an
                        // IllegalStateException will be thrown
                        output.Add(buf ?? Unpooled.Empty);
                        break;
                    case ST_CONTENT_CHUNK:
                        if (buf != null)
                        {
                            // We allocated a buffer so add it now.
                            output.Add(buf);
                        }

                        this.EncodeChunkedContent(context, message, ContentLength(message), output);
                        break;
                    default:
                        throw new EncoderException($"unexpected state {this.state}: {StringUtil.SimpleClassName(message)}");
                }

                if (message is ILastHttpContent)
                {
                    this.state = ST_INIT;
                }
            }
            else if (buf != null)
            {
                output.Add(buf);
            }
        }

        protected void EncodeHeaders(HttpHeaders headers, IByteBuffer buf)
        {
            foreach (HeaderEntry<ICharSequence, ICharSequence> header in headers)
            {
                HttpHeadersEncoder.EncoderHeader(header.Key, header.Value, buf);
            }
        }

        void EncodeChunkedContent(IChannelHandlerContext context, object message, long contentLength, ICollection<object> output)
        {
            if (contentLength > 0)
            {
                byte[] length = Encoding.ASCII.GetBytes(Convert.ToString(contentLength, 16));
                IByteBuffer buf = context.Allocator.Buffer(length.Length + 2);
                buf.WriteBytes(length);
                buf.WriteBytes(CRLF);
                output.Add(buf);
                output.Add(EncodeAndRetain(message));
                output.Add(CRLF_BUF.Duplicate());
            }

            var content = message as ILastHttpContent;
            if (content != null)
            {
                HttpHeaders headers = content.TrailingHeaders;
                if (headers.IsEmpty)
                {
                    output.Add(ZERO_CRLF_CRLF_BUF.Duplicate());
                }
                else
                {
                    IByteBuffer buf = context.Allocator.Buffer();
                    buf.WriteBytes(ZERO_CRLF);

                    try
                    {
                        this.EncodeHeaders(headers, buf);
                    }
                    catch
                    {
                        buf.Release();
                        throw;
                    }

                    buf.WriteBytes(CRLF);
                    output.Add(buf);
                }
            }
            else if (contentLength == 0)
            {
                // Need to produce some output otherwise an
                // IllegalstateException will be thrown
                output.Add(Unpooled.Empty);
            }
        }

        protected virtual bool IsContentAlwaysEmpty(T msg) => false;

        public override bool AcceptOutboundMessage(object msg) => msg is IHttpObject || msg is IByteBuffer || msg is IFileRegion;

        static object EncodeAndRetain(object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                return buffer.Retain();
            }
            var content = message as IHttpContent;
            if (content != null)
            {
                return content.Content.Retain();
            }
            var region = message as IFileRegion;
            if (region != null) 
            {
                return region.Retain();
            }

            throw new EncoderException($"unexpected message type: {StringUtil.SimpleClassName(message)}");
        }

        static long ContentLength(object message)
        {
            var content = message as IHttpContent;
            if (content != null)
            {
                return content.Content.ReadableBytes;
            }
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                return buffer.ReadableBytes;
            }
            var region = message as IFileRegion;
            if (region != null) 
            {
                return region.Count;
            }

            throw new EncoderException($"unexpected message type: {StringUtil.SimpleClassName(message)}");
        }

        protected internal abstract void EncodeInitialLine(IByteBuffer buf, T message);
    }
}
