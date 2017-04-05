// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http
{
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    static class HttpHeadersEncoder
    {
        public static void EncoderHeader(ICharSequence name, ICharSequence value, IByteBuffer buf)
        {
            int nameLen = name.Count;
            int valueLen = value.Count;
            int entryLen = nameLen + valueLen + 4;
            buf.EnsureWritable(entryLen);

            int offset = buf.WriterIndex;
            WriteAscii(buf, offset, name, nameLen);
            offset += nameLen;
            buf.SetByte(offset++, ':');
            buf.SetByte(offset++, ' ');
            WriteAscii(buf, offset, value, valueLen);

            offset += valueLen;
            buf.SetByte(offset++, '\r');
            buf.SetByte(offset++, '\n');
            buf.SetWriterIndex(offset);
        }

        static void WriteAscii(IByteBuffer buf, int offset, ICharSequence value, int valueLen)
        {
            if (value is AsciiString)
            {
                ByteBufferUtil.Copy((AsciiString)value, 0, buf, offset, valueLen);
            }
            else
            {
                WriteCharSequence(buf, offset, value, valueLen);
            }
        }

        static void WriteCharSequence(IByteBuffer buf, int offset, ICharSequence value, int valueLen)
        {
            for (int i = 0; i < valueLen; ++i)
            {
                buf.SetByte(offset++, AsciiString.CharToByte(value[i]));
            }
        }
    }
}
