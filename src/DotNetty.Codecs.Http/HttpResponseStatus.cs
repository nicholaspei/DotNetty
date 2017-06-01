﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    public class HttpResponseStatus : IComparable<HttpResponseStatus>
    {
        /**
          * 100 Continue
          */
        public static readonly HttpResponseStatus Continue = NewStatus(100, "Continue");

        /**
         * 101 Switching Protocols
         */
        public static readonly HttpResponseStatus SwitchingProtocols = NewStatus(101, "Switching Protocols");

        /**
         * 102 Processing (WebDAV, RFC2518)
         */
        public static readonly HttpResponseStatus Processing = NewStatus(102, "Processing");

        /**
         * 200 OK
         */
        public static readonly HttpResponseStatus OK = NewStatus(200, "OK");

        /**
         * 201 Created
         */
        public static readonly HttpResponseStatus Created = NewStatus(201, "Created");

        /**
         * 202 Accepted
         */
        public static readonly HttpResponseStatus Accepted = NewStatus(202, "Accepted");

        /**
         * 203 Non-Authoritative Information (since HTTP/1.1)
         */
        public static readonly HttpResponseStatus NonAuthoritativeInformation = NewStatus(203, "Non-Authoritative Information");

        /**
         * 204 No Content
         */
        public static readonly HttpResponseStatus NoContent = NewStatus(204, "No Content");

        /**
         * 205 Reset Content
         */
        public static readonly HttpResponseStatus ResetContent = NewStatus(205, "Reset Content");

        /**
         * 206 Partial Content
         */
        public static readonly HttpResponseStatus PartialContent = NewStatus(206, "Partial Content");

        /**
         * 207 Multi-Status (WebDAV, RFC2518)
         */
        public static readonly HttpResponseStatus MultiStatus = NewStatus(207, "Multi-Status");

        /**
         * 300 Multiple Choices
         */
        public static readonly HttpResponseStatus MultipleChoices = NewStatus(300, "Multiple Choices");

        /**
         * 301 Moved Permanently
         */
        public static readonly HttpResponseStatus MovedPermanently = NewStatus(301, "Moved Permanently");

        /**
         * 302 Found
         */
        public static readonly HttpResponseStatus Found = NewStatus(302, "Found");

        /**
         * 303 See Other (since HTTP/1.1)
         */
        public static readonly HttpResponseStatus SeeOther = NewStatus(303, "See Other");

        /**
         * 304 Not Modified
         */
        public static readonly HttpResponseStatus NotModified = NewStatus(304, "Not Modified");

        /**
         * 305 Use Proxy (since HTTP/1.1)
         */
        public static readonly HttpResponseStatus UseProxy = NewStatus(305, "Use Proxy");

        /**
         * 307 Temporary Redirect (since HTTP/1.1)
         */
        public static readonly HttpResponseStatus TemporaryRedirect = NewStatus(307, "Temporary Redirect");

        /**
         * 308 Permanent Redirect (RFC7538)
         */
        public static readonly HttpResponseStatus PermanentRedirect = NewStatus(308, "Permanent Redirect");

        /**
         * 400 Bad Request
         */
        public static readonly HttpResponseStatus BadRequest = NewStatus(400, "Bad Request");

        /**
         * 401 Unauthorized
         */
        public static readonly HttpResponseStatus Unauthorized = NewStatus(401, "Unauthorized");

        /**
         * 402 Payment Required
         */
        public static readonly HttpResponseStatus PaymentRequired = NewStatus(402, "Payment Required");

        /**
         * 403 Forbidden
         */
        public static readonly HttpResponseStatus Forbidden = NewStatus(403, "Forbidden");

        /**
         * 404 Not Found
         */
        public static readonly HttpResponseStatus NotFound = NewStatus(404, "Not Found");

        /**
         * 405 Method Not Allowed
         */
        public static readonly HttpResponseStatus MethodNotAllowed = NewStatus(405, "Method Not Allowed");

        /**
         * 406 Not Acceptable
         */
        public static readonly HttpResponseStatus NotAcceptable = NewStatus(406, "Not Acceptable");

        /**
         * 407 Proxy Authentication Required
         */
        public static readonly HttpResponseStatus ProxyAuthenticationRequired =
            NewStatus(407, "Proxy Authentication Required");

        /**
         * 408 Request Timeout
         */
        public static readonly HttpResponseStatus RequestTimeout = NewStatus(408, "Request Timeout");

        /**
         * 409 Conflict
         */
        public static readonly HttpResponseStatus Conflict = NewStatus(409, "Conflict");

        /**
         * 410 Gone
         */
        public static readonly HttpResponseStatus Gone = NewStatus(410, "Gone");

        /**
         * 411 Length Required
         */
        public static readonly HttpResponseStatus LengthRequired = NewStatus(411, "Length Required");

        /**
         * 412 Precondition Failed
         */
        public static readonly HttpResponseStatus PreconditionFailed = NewStatus(412, "Precondition Failed");

        /**
         * 413 Request Entity Too Large
         */
        public static readonly HttpResponseStatus RequestEntityTooLarge = NewStatus(413, "Request Entity Too Large");

        /**
         * 414 Request-URI Too Long
         */
        public static readonly HttpResponseStatus RequestUriTooLong = NewStatus(414, "Request-URI Too Long");

        /**
         * 415 Unsupported Media Type
         */
        public static readonly HttpResponseStatus UnsupportedMediaType = NewStatus(415, "Unsupported Media Type");

        /**
         * 416 Requested Range Not Satisfiable
         */
        public static readonly HttpResponseStatus RequestedRangeNotSatisfiable = NewStatus(416, "Requested Range Not Satisfiable");

        /**
         * 417 Expectation Failed
         */
        public static readonly HttpResponseStatus ExpectationFailed = NewStatus(417, "Expectation Failed");

        /**
         * 421 Misdirected Request
         *
         * <a href="https://tools.ietf.org/html/draft-ietf-httpbis-http2-15#section-9.1.2">421 Status Code</a>
         */
        public static readonly HttpResponseStatus MisdirectedRequest = NewStatus(421, "Misdirected Request");

        /**
         * 422 Unprocessable Entity (WebDAV, RFC4918)
         */
        public static readonly HttpResponseStatus UnprocessableEntity = NewStatus(422, "Unprocessable Entity");

        /**
         * 423 Locked (WebDAV, RFC4918)
         */
        public static readonly HttpResponseStatus Locked = NewStatus(423, "Locked");

        /**
         * 424 Failed Dependency (WebDAV, RFC4918)
         */
        public static readonly HttpResponseStatus FailedDependency = NewStatus(424, "Failed Dependency");

        /**
         * 425 Unordered Collection (WebDAV, RFC3648)
         */
        public static readonly HttpResponseStatus UnorderedCollection = NewStatus(425, "Unordered Collection");

        /**
         * 426 Upgrade Required (RFC2817)
         */
        public static readonly HttpResponseStatus UpgradeRequired = NewStatus(426, "Upgrade Required");

        /**
         * 428 Precondition Required (RFC6585)
         */
        public static readonly HttpResponseStatus PreconditionRequired = NewStatus(428, "Precondition Required");

        /**
         * 429 Too Many Requests (RFC6585)
         */
        public static readonly HttpResponseStatus TooManyRequests = NewStatus(429, "Too Many Requests");

        /**
         * 431 Request Header Fields Too Large (RFC6585)
         */
        public static readonly HttpResponseStatus RequestHeaderFieldsTooLarge =
            NewStatus(431, "Request Header Fields Too Large");

        /**
         * 500 Internal Server Error
         */
        public static readonly HttpResponseStatus InternalServerError = NewStatus(500, "Internal Server Error");

        /**
         * 501 Not Implemented
         */
        public static readonly HttpResponseStatus NotImplemented = NewStatus(501, "Not Implemented");

        /**
         * 502 Bad Gateway
         */
        public static readonly HttpResponseStatus BadGateway = NewStatus(502, "Bad Gateway");

        /**
         * 503 Service Unavailable
         */
        public static readonly HttpResponseStatus ServiceUnavailable = NewStatus(503, "Service Unavailable");

        /**
         * 504 Gateway Timeout
         */
        public static readonly HttpResponseStatus GatewayTimeout = NewStatus(504, "Gateway Timeout");

        /**
         * 505 HTTP Version Not Supported
         */
        public static readonly HttpResponseStatus HttpVersionNotSupported = NewStatus(505, "HTTP Version Not Supported");

        /**
         * 506 Variant Also Negotiates (RFC2295)
         */
        public static readonly HttpResponseStatus VariantAlsoNegotiates = NewStatus(506, "Variant Also Negotiates");

        /**
         * 507 Insufficient Storage (WebDAV, RFC4918)
         */
        public static readonly HttpResponseStatus InsufficientStorage = NewStatus(507, "Insufficient Storage");

        /**
         * 510 Not Extended (RFC2774)
         */
        public static readonly HttpResponseStatus NotExtended = NewStatus(510, "Not Extended");

        /**
         * 511 Network Authentication Required (RFC6585)
         */
        public static readonly HttpResponseStatus NetworkAuthenticationRequired = NewStatus(511, "Network Authentication Required");

        /**
             * Returns the {@link HttpResponseStatus} represented by the specified code.
             * If the specified code is a standard HTTP getStatus code, a cached instance
             * will be returned.  Otherwise, a new instance will be returned.
             */
        public static HttpResponseStatus ValueOf(int code)
        {
            switch (code)
            {
                case 100:
                    return Continue;
                case 101:
                    return SwitchingProtocols;
                case 102:
                    return Processing;
                case 200:
                    return OK;
                case 201:
                    return Created;
                case 202:
                    return Accepted;
                case 203:
                    return NonAuthoritativeInformation;
                case 204:
                    return NoContent;
                case 205:
                    return ResetContent;
                case 206:
                    return PartialContent;
                case 207:
                    return MultiStatus;
                case 300:
                    return MultipleChoices;
                case 301:
                    return MovedPermanently;
                case 302:
                    return Found;
                case 303:
                    return SeeOther;
                case 304:
                    return NotModified;
                case 305:
                    return UseProxy;
                case 307:
                    return TemporaryRedirect;
                case 308:
                    return PermanentRedirect;
                case 400:
                    return BadRequest;
                case 401:
                    return Unauthorized;
                case 402:
                    return PaymentRequired;
                case 403:
                    return Forbidden;
                case 404:
                    return NotFound;
                case 405:
                    return MethodNotAllowed;
                case 406:
                    return NotAcceptable;
                case 407:
                    return ProxyAuthenticationRequired;
                case 408:
                    return RequestTimeout;
                case 409:
                    return Conflict;
                case 410:
                    return Gone;
                case 411:
                    return LengthRequired;
                case 412:
                    return PreconditionFailed;
                case 413:
                    return RequestEntityTooLarge;
                case 414:
                    return RequestUriTooLong;
                case 415:
                    return UnsupportedMediaType;
                case 416:
                    return RequestedRangeNotSatisfiable;
                case 417:
                    return ExpectationFailed;
                case 421:
                    return MisdirectedRequest;
                case 422:
                    return UnprocessableEntity;
                case 423:
                    return Locked;
                case 424:
                    return FailedDependency;
                case 425:
                    return UnorderedCollection;
                case 426:
                    return UpgradeRequired;
                case 428:
                    return PreconditionRequired;
                case 429:
                    return TooManyRequests;
                case 431:
                    return RequestHeaderFieldsTooLarge;
                case 500:
                    return InternalServerError;
                case 501:
                    return NotImplemented;
                case 502:
                    return BadGateway;
                case 503:
                    return ServiceUnavailable;
                case 504:
                    return GatewayTimeout;
                case 505:
                    return HttpVersionNotSupported;
                case 506:
                    return VariantAlsoNegotiates;
                case 507:
                    return InsufficientStorage;
                case 510:
                    return NotExtended;
                case 511:
                    return NetworkAuthenticationRequired;
            }

            return new HttpResponseStatus(code);
        }

        public static HttpResponseStatus ParseLine(ICharSequence line)
        {
            string status = line.ToString();
            try
            {
                int space = status.IndexOf(' ');
                if (space == -1)
                {
                    return ValueOf(int.Parse(status));
                }
                else
                {
                    int code = int.Parse(status.Substring(0, space));
                    string reasonPhrase = status.Substring(space + 1);
                    HttpResponseStatus responseStatus = ValueOf(code);
                    if (responseStatus.ReasonPhrase.Equals(reasonPhrase))
                    {
                        return responseStatus;
                    }
                    else
                    {
                        return new HttpResponseStatus(code, reasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"malformed status line: {status} ", e);
            }
        }

        sealed class HttpStatusLineProcessor : ByteProcessor
        {
            const byte AsciiSpace = (byte)' ';
            readonly AsciiString asciiString;
            int i;

            /**
             * 0 = New or havn't seen {@link #ASCII_SPACE}.
             * 1 = Last byte was {@link #ASCII_SPACE}.
             * 2 = Terminal State. Processed the byte after {@link #ASCII_SPACE}, and parsed the status line.
             * 3 = Terminal State. There was no byte after {@link #ASCII_SPACE} but status has been parsed with what we saw.
             */
            int state;
            HttpResponseStatus status;

            public HttpStatusLineProcessor(AsciiString asciiString)
            {
                this.asciiString = asciiString;
            }

            public override bool Process(byte value)
            {
                switch (this.state)
                {
                    case 0:
                        if (value == AsciiSpace)
                        {
                            this.state = 1;
                        }
                        break;
                    case 1:
                        this.ParseStatus(this.i);
                        this.state = 2;
                        return false;
                }

                ++this.i;

                return true;
            }

            void ParseStatus(int codeEnd)
            {
                int code = this.asciiString.ParseInt(0, codeEnd);
                this.status = ValueOf(code);
                if (codeEnd < this.asciiString.Count)
                {
                    string actualReason = this.asciiString.ToString(codeEnd + 1, this.asciiString.Count);
                    if (!this.status.ReasonPhrase.Equals(actualReason))
                    {
                        this.status = new HttpResponseStatus(code, actualReason);
                    }
                }
            }

            public HttpResponseStatus Status()
            {
                if (this.state <= 1)
                {
                    this.ParseStatus(this.asciiString.Count);
                    this.state = 3;
                }

                return this.status;
            }
        }

        public static HttpResponseStatus ParseLine(AsciiString line)
        {
            try
            {
                var processor = new HttpStatusLineProcessor(line);
                line.ForEachByte(processor);
                HttpResponseStatus status = processor.Status();
                if (status == null)
                {
                    throw new ArgumentException("unable to get status after parsing input");
                }
                return status;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"malformed status line: {line}", e);
            }
        }

        static HttpResponseStatus NewStatus(int statusCode, string reasonPhrase) => 
            new HttpResponseStatus(statusCode, reasonPhrase, true);

        readonly byte[] bytes;

        HttpResponseStatus(int code) : this(code, 
            $"{HttpStatusClass.ValueOf(code).DefaultReasonPhrase} ({code})")
        {
        }

        public HttpResponseStatus(int code, string reasonPhrase, bool bytes = false)
        {
            Contract.Requires(code >= 0);
            Contract.Requires(reasonPhrase != null);

            foreach (char c in reasonPhrase)
            {
                // Check prohibited characters.
                switch (c)
                {
                    case '\n':
                    case '\r':
                        throw new ArgumentException($"reasonPhrase contains one of the following prohibited characters: \\r\\n: {reasonPhrase}");
                }
            }

            this.Code = code;
            this.CodeAsText = new AsciiString(Convert.ToString(code));
            this.ReasonPhrase = reasonPhrase;
            this.bytes = bytes ? Encoding.ASCII.GetBytes($"{code} {reasonPhrase}") : null;
            this.CodeClass = HttpStatusClass.ValueOf(code);
        }

        public int Code { get; }

        public AsciiString CodeAsText { get; }

        public string ReasonPhrase { get; }

        public HttpStatusClass CodeClass { get; }

        public override int GetHashCode() => this.Code;

        public override bool Equals(object obj) => (obj is HttpResponseStatus) && this.Code == ((HttpResponseStatus)obj).Code;

        public int CompareTo(HttpResponseStatus other) => this.Code - other.Code;

        public override string ToString() => 
            new StringBuilder(this.ReasonPhrase.Length + 5)
            .Append(this.Code)
            .Append(' ')
            .Append(this.ReasonPhrase)
            .ToString();

        internal void Encode(IByteBuffer buf)
        {
            if (this.bytes == null)
            {
                HttpUtil.EncodeAscii0(Convert.ToString(this.Code), buf);
                buf.WriteByte(HttpConstants.HorizontalSpace);
                HttpUtil.EncodeAscii0(this.ReasonPhrase, buf);
            }
            else
            {
                buf.WriteBytes(this.bytes);
            }
        }
    }
}
