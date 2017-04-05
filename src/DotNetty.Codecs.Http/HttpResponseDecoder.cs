// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    public class HttpResponseDecoder : HttpObjectDecoder
    {
        static readonly HttpResponseStatus UnknownStatus = new HttpResponseStatus(999, "Unknown");

        public HttpResponseDecoder()
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize) 
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize)
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, true, validateHeaders)
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders, int initialBufferSize)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, true, validateHeaders, initialBufferSize)
        {
        }

        protected override IHttpMessage CreateMessage(string[] initialLine) => 
            new DefaultHttpResponse(HttpVersion.ValueOf(initialLine[0]), new HttpResponseStatus(int.Parse(initialLine[1]), initialLine[2]), this.ValidateHeaders);

        protected override IHttpMessage CreateInvalidMessage() => 
            new DefaultFullHttpResponse(HttpVersion.Http10, UnknownStatus, this.ValidateHeaders);

        protected override bool IsDecodingRequest() => false;
    }
}
