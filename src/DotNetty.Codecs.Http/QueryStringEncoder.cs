// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Text;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// * Creates an URL-encoded URI from a path string and key-value parameter pairs.
    /// * This encoder is for one time use only.  Create a new instance for each URI.
    /// *
    /// * <pre>
    /// * {@link QueryStringEncoder} encoder = new {@link QueryStringEncoder}("/hello");
    /// * encoder.addParam("recipient", "world");
    /// * assert encoder.toString().equals("/hello?recipient=world");
    /// * </pre>
    /// * @see QueryStringDecoder
    /// </summary>
    public class QueryStringEncoder
    {
        const string EncodedSpace = "%20";

        readonly string uri;
        readonly Encoding encoding;
        readonly List<Param> parameters = new List<Param>();

        public QueryStringEncoder(string uri) : this(uri, HttpConstants.DefaultEncoding)
        {
        }

        public QueryStringEncoder(string uri, Encoding encoding)
        {
            Contract.Requires(uri !=  null); 
            Contract.Requires(encoding != null);

            this.uri = uri;
            this.encoding = encoding;
        }

        public void AddParam(string name, string value)
        {
            Contract.Requires(name != null);

            this.parameters.Add(new Param(name, value));
        }

        public string ToUriString() => this.ToString();

        public override string ToString()
        {
            if (this.parameters.Count == 0)
            {
                return this.uri;
            }

            var sb = new StringBuilder(this.uri);
            sb.Append('?');

            for (int i = 0; i < this.parameters.Count; i++)
            {
                Param param = this.parameters[i];

                sb.Append(EncodeComponent(param.Name, this.encoding));
                if (param.Value != null)
                {
                    sb.Append('=');
                    sb.Append(EncodeComponent(param.Value, this.encoding));
                }

                if (i != this.parameters.Count - 1)
                {
                    sb.Append('&');
                }
            }

            return sb.ToString();
        }

        static string EncodeComponent(string s, Encoding encoding)
        {
            var buf = new StringBuilder();

            int count = encoding.GetMaxByteCount(1);
            var bytes = new byte[count];
            var array = new char[1];

            foreach (char ch in s)
            {
                if (ch >= 'a' && ch <= 'z'
                    || ch >= 'A' && ch <= 'Z'
                    || ch >= '0' && ch <= '9')
                {
                    buf.Append(ch);
                }
                else
                {
                    if (ch == '+')
                    {
                        buf.Append(EncodedSpace);
                    }
                    else
                    {
                        array[0] = ch;
                        count = encoding.GetBytes(array, 0, 1, bytes, 0);
                        for (int i = 0; i < count; i++)
                        {
                            buf.Append('%');
                            buf.Append(CharUtil.Digits[(bytes[i] & 0xf0) >> 4]);
                            buf.Append(CharUtil.Digits[bytes[i] & 0xf]);
                        }
                    }
                }
            }

            return buf.ToString();
        }

        sealed class Param
        {
            public Param(string name, string value)
            {
                this.Value = value;
                this.Name = name;
            }

            public string Name { get; }

            public string Value { get; }
        }
    }
}
