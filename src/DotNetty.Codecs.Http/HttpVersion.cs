// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Text;
    using System.Text.RegularExpressions;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    public class HttpVersion : IComparable<HttpVersion>, IComparable
    {
        static readonly Regex VersionPattern = new Regex("(\\S+)/(\\d+)\\.(\\d+)");

        static readonly string Http10String = "HTTP/1.0";
        static readonly string Http11String = "HTTP/1.1";

        public static readonly HttpVersion Http10 = new HttpVersion("HTTP", 1, 0, false, true);
        public static readonly HttpVersion Http11 = new HttpVersion("HTTP", 1, 1, true, true);

        public static HttpVersion ValueOf(string text)
        {
            Contract.Requires(text != null);

            text = text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("text is empty");
            }

            // Try to match without convert to uppercase first as this is what 99% of all clients
            // will send anyway. Also there is a change to the RFC to make it clear that it is
            // expected to be case-sensitive
            //
            // See:
            // * http://trac.tools.ietf.org/wg/httpbis/trac/ticket/1
            // * http://trac.tools.ietf.org/wg/httpbis/trac/wiki
            //
            return Version0(text) ?? new HttpVersion(text, true);
        }

        static HttpVersion Version0(string text)
        {
            if (Http11String.Equals(text))
            {
                return Http11;
            }

            if (Http10String.Equals(text))
            {
                return Http10;
            }

            return null;
        }

        readonly byte[] bytes;

        public HttpVersion(string text, bool keepAliveDefault)
        {
            Contract.Requires(text != null);

            text = text.Trim().ToUpper();
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("empty text");
            }

            MatchCollection m = VersionPattern.Matches(text);
            if (m.Count == 0)
            {
                throw new ArgumentException("invalid version format: " + text);
            }

            this.ProtocolName = m[1].Value;
            this.MajorVersion = int.Parse(m[2].Value);
            this.MinorVersion = int.Parse(m[3].Value);
            this.Text = this.ProtocolName + '/' + this.MajorVersion + '.' + this.MinorVersion;
            this.IsKeepAliveDefault = keepAliveDefault;
            this.bytes = null;
        }

        HttpVersion(string protocolName, int majorVersion, int minorVersion, bool keepAliveDefault, bool bytes = false)
        {
            Contract.Requires(protocolName != null);
            Contract.Requires(majorVersion >= 0 && minorVersion >= 0);

            protocolName = protocolName.Trim().ToUpper();
            if (string.IsNullOrEmpty(protocolName))
            {
                throw new ArgumentException("empty protocolName");
            }

            foreach (char t in protocolName)
            {
                if (CharUtil.IsISOControl(t) || char.IsWhiteSpace(t))
                {
                    throw new ArgumentException("invalid character in protocolName");
                }
            }

            this.ProtocolName = protocolName;
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.Text = protocolName + '/' + majorVersion + '.' + minorVersion;
            this.IsKeepAliveDefault = keepAliveDefault;

            this.bytes = bytes ? Encoding.ASCII.GetBytes(this.Text) : null;
        }

        public string ProtocolName { get; }

        public int MajorVersion { get; }

        public int MinorVersion { get; }

        public string Text { get; }

        public bool IsKeepAliveDefault { get; }

        public override string ToString() => this.Text;

        public override int GetHashCode() => (this.ProtocolName.GetHashCode() * 31 + this.MajorVersion) * 31 + this.MinorVersion;

        public override bool Equals(object obj)
        {
            if (!(obj is HttpVersion)) {
                return false;
            }

            var that = (HttpVersion)obj;
            return this.MinorVersion == that.MinorVersion 
                && this.MajorVersion == that.MajorVersion 
                && this.ProtocolName.Equals(that.ProtocolName);
        }

        public int CompareTo(HttpVersion other)
        {
            int v = string.CompareOrdinal(this.ProtocolName, other.ProtocolName);
            if (v != 0)
            {
                return v;
            }

            v = this.MajorVersion - other.MajorVersion;
            if (v != 0)
            {
                return v;
            }

            return this.MinorVersion - other.MinorVersion;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (!(obj is HttpVersion))
            {
                throw new ArgumentException($"{nameof(obj)} must be of {nameof(HttpVersion)} type");
            }

            return this.CompareTo((HttpVersion)obj);
        }

        internal void Encode(IByteBuffer buf)
        {
            if (this.bytes == null)
            {
                HttpUtil.EncodeAscii0(this.Text, buf);
            }
            else
            {
                buf.WriteBytes(this.bytes);
            }
        }
    }
}
