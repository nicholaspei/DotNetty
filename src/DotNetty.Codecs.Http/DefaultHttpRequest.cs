// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Text;

    public class DefaultHttpRequest : DefaultHttpMessage, IHttpRequest
    {
        const int HashCodePrime = 31;

        HttpMethod method;
        string uri;

        public DefaultHttpRequest(HttpVersion version, HttpMethod method, string uri, bool validateHeaders = true)
            : base(version, validateHeaders)
        {
            Contract.Requires(method != null);
            Contract.Requires(uri != null);

            this.method = method;
            this.uri = uri;
        }

        public DefaultHttpRequest(HttpVersion version, HttpMethod method, string uri, HttpHeaders headers) 
            : base(version, headers)
        {
            Contract.Requires(method != null);
            Contract.Requires(uri != null);

            this.method = method;
            this.uri = uri;
        }

        public HttpMethod Method
        {
            get
            {
                return this.method;
            }
            set
            {
                Contract.Requires(value != null);
                this.method = value;
            }
        }

        public string Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                Contract.Requires(value != null);
                this.uri = value;
            }
        }

        public override int GetHashCode()
        {
            int result = 1;
            // ReSharper disable NonReadonlyMemberInGetHashCode
            result = HashCodePrime * result + this.method.GetHashCode();
            result = HashCodePrime * result + this.uri.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
            result = HashCodePrime * result + base.GetHashCode();

            return result;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DefaultHttpRequest)) {
                return false;
            }

            var other = (DefaultHttpRequest)obj;
            
            return this.method.Equals(other.method) 
                && this.uri.Equals(other.uri, StringComparison.OrdinalIgnoreCase) 
                && base.Equals(obj);
        }

        public override string ToString() => 
            HttpMessageUtil.AppendRequest(new StringBuilder(256), this).ToString();
    }
}
