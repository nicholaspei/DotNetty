// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using DotNetty.Codecs;
    using DotNetty.Common.Utilities;

    public class DefaultHttpHeaders : HttpHeaders
    {
        const int HighestInvalidValueCharMask = ~15;

        static readonly HeaderValueConverter DefaultHeaderValueConverter = new HeaderValueConverter();
        static readonly HeaderValueConverterAndValidator DefaultHeaderValueConverterAndValidator = new HeaderValueConverterAndValidator();

        internal static readonly INameValidator<ICharSequence> HttpNameValidator = new NameValidator();
        internal static readonly INameValidator<ICharSequence> NotNullValidator = new NullNameValidator<ICharSequence>();

        sealed class NameValidator : ByteProcessor, INameValidator<ICharSequence>
        {
            public void ValidateName(ICharSequence name)
            {
                Contract.Requires(name != null && name.Count > 0);

                var s = name as AsciiString;
                if (s != null)
                {
                    s.ForEachByte(this);
                }
                else
                {
                    // Go through each character in the name

                    // ReSharper disable once ForCanBeConvertedToForeach
                    // Avoid new enumerator instance
                    for (int index = 0; index < name.Count; ++index)
                    {
                        ValidateHeaderNameElement(name[index]);
                    }
                }
            }

            public override bool Process(byte value)
            {
                ValidateHeaderNameElement(value);

                return true;
            }
        }

        static void ValidateHeaderNameElement(byte value)
        {
            switch (value)
            {
                case 0x00:
                case 0x09: //'\t':
                case 0x0a: //'\n':
                case 0x0b:
                case 0x0c: //'\f':
                case 0x0d: //'\r':
                case 0x20: //' ':
                case 0x2c: //',':
                case 0x3a: //':':
                case 0x3b: //';':
                case 0x3d: //'=':
                    throw new ArgumentException(
                        $"a header name cannot contain the following prohibited characters: =,;: \\t\\r\\n\\v\\f: {value}");
            }

            // Check to see if the character is not an ASCII character, or invalid
            if (value > 127)
            {
                throw new ArgumentException($"a header name cannot contain non-ASCII character: {value}");
            }
        }

        static void ValidateHeaderNameElement(char value)
        {
            switch (value)
            {
                case '\x00':
                case '\t':
                case '\n':
                case '\x0b':
                case '\f':
                case '\r':
                case ' ':
                case ',':
                case ':':
                case ';':
                case '=':
                    throw new ArgumentException(
                       $"a header name cannot contain the following prohibited characters: =,;: \\t\\r\\n\\v\\f: {value}");
            }

            // Check to see if the character is not an ASCII character, or invalid
            if (value > 127)
            {
                throw new ArgumentException($"a header name cannot contain non-ASCII character: {value}");
            }
        }

        readonly DefaultHeaders<ICharSequence, ICharSequence> headers;

        public DefaultHttpHeaders(bool validate = true) 
            : this(validate, NewNameValidator(validate))
        {
        }

        protected DefaultHttpHeaders(bool validate, INameValidator<ICharSequence> nameValidator) 
            : this(new DefaultHeaders<ICharSequence, ICharSequence>(AsciiString.CaseInsensitiveHasher, NewValueConverter(validate), nameValidator))
        {
        }

        protected DefaultHttpHeaders(DefaultHeaders<ICharSequence, ICharSequence> headers)
        {
            this.headers = headers;
        }

        public override HttpHeaders Add(HttpHeaders httpHeaders)
        {
            var defaultHttpHeaders = httpHeaders as DefaultHttpHeaders;
            if (defaultHttpHeaders != null)
            {
                this.headers.Add(defaultHttpHeaders.headers);
                return this;
            }

            return base.Add(httpHeaders);
        }

        public override HttpHeaders Set(HttpHeaders httpHeaders)
        {
            var defaultHttpHeaders = httpHeaders as DefaultHttpHeaders;
            if (defaultHttpHeaders != null)
            {
                this.headers.Set(defaultHttpHeaders.headers);
                return this;
            }

            return base.Set(httpHeaders);
        }

        public override HttpHeaders Add(ICharSequence name, object value)
        {
            Contract.Requires(value != null);

            this.headers.AddObject(name, value);
            return this;
        }

        public override HttpHeaders AddInt(ICharSequence name, int value)
        {
            this.headers.AddInt(name, value);
            return this;
        }

        public override HttpHeaders AddShort(ICharSequence name, short value)
        {
            this.headers.AddShort(name, value);
            return this;
        }

        public override HttpHeaders Remove(ICharSequence name)
        {
            this.headers.Remove(name);
            return this;
        }

        public override HttpHeaders Set(ICharSequence name, object value)
        {
            Contract.Requires(name != null && name.Count > 0);
            Contract.Requires(value != null);

            this.headers.SetObject(name, value);
            return this;
        }

        public override HttpHeaders Set(ICharSequence name, IEnumerable<object> values)
        {
            Contract.Requires(name != null && name.Count > 0);
            Contract.Requires(values != null);

            this.headers.SetObject(name, values);
            return this;
        }

        public override HttpHeaders SetInt(ICharSequence name, int value)
        {
            this.headers.SetInt(name, value);
            return this;
        }

        public override HttpHeaders SetShort(ICharSequence name, short value)
        {
            this.headers.SetShort(name, value);
            return this;
        }

        public override HttpHeaders Clear()
        {
            this.headers.Clear();
            return this;
        }

        public override ICharSequence Get(ICharSequence name) => this.headers.Get(name);

        public override int? GetInt(ICharSequence name) => this.headers.GetInt(name);

        public override int GetInt(ICharSequence name, int defaultValue) => this.headers.GetInt(name, defaultValue);

        public override short? GetShort(ICharSequence name) => this.headers.GetShort(name);

        public override short GetShort(ICharSequence name, short defaultValue) => this.headers.GetShort(name, defaultValue);

        public override long? GetTimeMillis(ICharSequence name) => this.headers.GetTimeMillis(name);

        public override long GetTimeMillis(ICharSequence name, long defaultValue) => this.headers.GetTimeMillis(name, defaultValue);

        public override IList<ICharSequence> GetAll(ICharSequence name) => this.headers.GetAll(name);

        public override bool Contains(ICharSequence name) => this.headers.Contains(name);

        public override bool IsEmpty => this.headers.IsEmpty;

        public override int Size => this.headers.Size;

        public override bool Contains(ICharSequence name, ICharSequence value, bool ignoreCase) => 
            this.headers.Contains(name, value, 
                ignoreCase ? AsciiString.CaseInsensitiveHasher : AsciiString.CaseSensitiveHasher);

        protected static IValueConverter<ICharSequence> NewValueConverter(bool validate) => 
            validate ? DefaultHeaderValueConverterAndValidator : DefaultHeaderValueConverter;

        public override ISet<ICharSequence> Names() => this.headers.Names();

        public override bool Equals(object obj) => 
            obj is DefaultHttpHeaders
            && this.headers.Equals(((DefaultHttpHeaders)obj).headers, AsciiString.CaseSensitiveHasher);

        public override int GetHashCode() => this.headers.HashCode(AsciiString.CaseSensitiveHasher);

        class HeaderValueConverter : CharSequenceValueConverter
        {
            public override ICharSequence ConvertObject(object value)
            {
                var seq = value as ICharSequence;
                if (seq != null)
                {
                    return seq;
                }

                if (value is DateTime)
                {
                    return (StringCharSequence)DateFormatter.Format((DateTime)value);
                }

                return (StringCharSequence)value.ToString();
            }
        }

        sealed class HeaderValueConverterAndValidator : HeaderValueConverter
        {
            public override ICharSequence ConvertObject(object value)
            {
                ICharSequence seq = base.ConvertObject(value);
                int state = 0;

                // Start looping through each of the character
                // ReSharper disable once ForCanBeConvertedToForeach
                // Avoid enumerator allocation
                for (int index = 0; index < seq.Count; index++)
                {
                    state = ValidateValueChar(seq, state, seq[index]);
                }

                if (state != 0)
                {
                    throw new ArgumentException($"a header value must not end with '\\r' or '\\n':{seq}");
                }

                return seq;
            }

            static int ValidateValueChar(ICharSequence seq, int state, char character)
            {
                /*
                 * State:
                 * 0: Previous character was neither CR nor LF
                 * 1: The previous character was CR
                 * 2: The previous character was LF
                 */
                if ((character & HighestInvalidValueCharMask) == 0)
                {
                    // Check the absolutely prohibited characters.
                    switch (character)
                    {
                        case '\x00': // NULL
                            throw new ArgumentException($"a header value contains a prohibited character '\0': {seq}");
                        case '\x0b': // Vertical tab
                            throw new ArgumentException($"a header value contains a prohibited character '\\v': {seq}");
                        case '\f':
                            throw new ArgumentException($"a header value contains a prohibited character '\\f': {seq}");
                    }
                }

                // Check the CRLF (HT | SP) pattern
                switch (state)
                {
                    case 0:
                        switch (character)
                        {
                            case '\r':
                                return 1;
                            case '\n':
                                return 2;
                        }
                        break;
                    case 1:
                        switch (character)
                        {
                            case '\n':
                                return 2;
                            default:
                                throw new ArgumentException($"only '\\n' is allowed after '\\r': {seq}");
                        }
                    case 2:
                        switch (character)
                        {
                            case '\t':
                            case ' ':
                                return 0;
                            default:
                                throw new ArgumentException($"only ' ' and '\\t' are allowed after '\\n': {seq}");
                        }
                }

                return state;
            }
        }

        protected static INameValidator<ICharSequence> NewNameValidator(bool validate) => 
            validate ? HttpNameValidator : NotNullValidator;

        public override IEnumerator<HeaderEntry<ICharSequence, ICharSequence>> GetEnumerator() => this.headers.GetEnumerator();
    }
}
