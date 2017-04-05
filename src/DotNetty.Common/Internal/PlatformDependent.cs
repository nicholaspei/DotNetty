// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using DotNetty.Common.Utilities;

    public static class PlatformDependent
    {
        static int seed = (int)(Stopwatch.GetTimestamp() & 0xFFFFFFFF); //used to safly cast long to int, because the timestamp returned is long and it doesn't fit into an int
        static readonly ThreadLocal<Random> RandomProvider = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed))); //used to simulate java ThreadLocalRandom

        static readonly uint HASH_CODE_ASCII_SEED = 0xc2b2ae35;
        static readonly int HASH_CODE_C1 = 0x1b873593;
        static readonly int HASH_CODE_C2 = 0x1b873593;

        public static IQueue<T> NewFixedMpscQueue<T>(int capacity) where T : class => new MpscArrayQueue<T>(capacity);

        public static IQueue<T> NewMpscQueue<T>() where T : class => new CompatibleConcurrentQueue<T>();

        public static Random ThreadLocalRandom => RandomProvider.Value;

        public static bool ByteArrayEquals(byte[] bytes1, int startPos1, byte[] bytes2, int startPos2, int length)
        {
            int end = startPos1 + length;
            for (int i = startPos1, j = startPos2; i < end; ++i, ++j)
            {
                if (bytes1[i] != bytes2[j])
                {
                    return false;
                }
            }

            return true;
        }

        public static int HashCodeAscii(byte[] bytes, int startPos, int length)
        {
            int hash = (int)HASH_CODE_ASCII_SEED;
            int remainingBytes = length & 7;
            int end = startPos + remainingBytes;
            for (int i = startPos - 8 + length; i >= end; i -= 8)
            {
                hash = HashCodeAsciiCompute(ReadLong(bytes, i), hash);
            }
            switch (remainingBytes)
            {
                case 7:
                    return ((hash * HASH_CODE_C1 + HashCodeAsciiSanitize(bytes[startPos]))
                            * HASH_CODE_C2 + HashCodeAsciiSanitize(ReadShort(bytes, startPos + 1)))
                        * HASH_CODE_C1 + HashCodeAsciiSanitize(ReadInt(bytes, startPos + 3));
                case 6:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitize(ReadShort(bytes, startPos)))
                        * HASH_CODE_C2 + HashCodeAsciiSanitize(ReadInt(bytes, startPos + 2));
                case 5:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitize(bytes[startPos]))
                        * HASH_CODE_C2 + HashCodeAsciiSanitize(ReadInt(bytes, startPos + 1));
                case 4:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitize(ReadInt(bytes, startPos));
                case 3:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitize(bytes[startPos]))
                        * HASH_CODE_C2 + HashCodeAsciiSanitize(ReadShort(bytes, startPos + 1));
                case 2:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitize(ReadShort(bytes, startPos));
                case 1:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitize(bytes[startPos]);
                default:
                    return hash;
            }
        }

        static long ReadLong(byte[] bytes, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return (long)bytes[offset] << 56 |
                    ((long)bytes[offset + 1] & 0xff) << 48 |
                    ((long)bytes[offset + 2] & 0xff) << 40 |
                    ((long)bytes[offset + 3] & 0xff) << 32 |
                    ((long)bytes[offset + 4] & 0xff) << 24 |
                    ((long)bytes[offset + 5] & 0xff) << 16 |
                    ((long)bytes[offset + 6] & 0xff) << 8 |
                    (long)bytes[offset + 7] & 0xff;
            }

            return (long)bytes[offset] & 0xff |
                ((long)bytes[offset + 1] & 0xff) << 8 |
                ((long)bytes[offset + 2] & 0xff) << 16 |
                ((long)bytes[offset + 3] & 0xff) << 24 |
                ((long)bytes[offset + 4] & 0xff) << 32 |
                ((long)bytes[offset + 5] & 0xff) << 40 |
                ((long)bytes[offset + 6] & 0xff) << 48 |
                (long)bytes[offset + 7] << 56;
        }

        static int ReadInt(byte[] bytes, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return bytes[offset] << 24 |
                    (bytes[offset + 1] & 0xff) << 16 |
                    (bytes[offset + 2] & 0xff) << 8 |
                    bytes[offset + 3] & 0xff;
            }

            return bytes[offset] & 0xff |
                (bytes[offset + 1] & 0xff) << 8 |
                (bytes[offset + 2] & 0xff) << 16 |
                bytes[offset + 3] << 24;
        }

        static short ReadShort(byte[] bytes, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return (short)(bytes[offset] << 8 | (bytes[offset + 1] & 0xff));
            }

            return (short)(bytes[offset] & 0xff | (bytes[offset + 1] << 8));
        }

        // masking with 0x1f reduces the number of overall bits that impact the hash code but makes the hash
        // code the same regardless of character case (upper case or lower case hash is the same).
        static int HashCodeAsciiCompute(long value, int hash) => 
            hash * HASH_CODE_C1 +
            // Low order int
            HashCodeAsciiSanitize((int)value) * HASH_CODE_C2 +
            // High order int
            (int)((value & 0x1f1f1f1f00000000L) >> 32);

        public static int HashCodeAscii(ICharSequence bytes)
        {
            int hash = (int)HASH_CODE_ASCII_SEED;
            int remainingBytes = bytes.Count & 7;

            // Benchmarking shows that by just naively looping for inputs 8~31 bytes long we incur a relatively large
            // performance penalty (only achieve about 60% performance of loop which iterates over each char). So because
            // of this we take special provisions to unroll the looping for these conditions.
            switch (bytes.Count)
            {
                case 31:
                case 30:
                case 29:
                case 28:
                case 27:
                case 26:
                case 25:
                case 24:
                    hash = HashCodeAsciiCompute(bytes, bytes.Count - 24,
                        HashCodeAsciiCompute(bytes, bytes.Count - 16,
                            HashCodeAsciiCompute(bytes, bytes.Count - 8, hash)));
                    break;
                case 23:
                case 22:
                case 21:
                case 20:
                case 19:
                case 18:
                case 17:
                case 16:
                    hash = HashCodeAsciiCompute(bytes, bytes.Count - 16,
                        HashCodeAsciiCompute(bytes, bytes.Count - 8, hash));
                    break;
                case 15:
                case 14:
                case 13:
                case 12:
                case 11:
                case 10:
                case 9:
                case 8:
                    hash = HashCodeAsciiCompute(bytes, bytes.Count - 8, hash);
                    break;
                case 7:
                case 6:
                case 5:
                case 4:
                case 3:
                case 2:
                case 1:
                case 0:
                    break;
                default:
                    for (int i = bytes.Count - 8; i >= remainingBytes; i -= 8)
                    {
                        hash = HashCodeAsciiCompute(bytes, i, hash);
                    }
                    break;
            }
            switch (remainingBytes)
            {
                case 7:
                    return ((hash * HASH_CODE_C1 + HashCodeAsciiSanitizsByte(bytes[0]))
                            * HASH_CODE_C2 + HashCodeAsciiSanitizeShort(bytes, 1))
                        * HASH_CODE_C1 + HashCodeAsciiSanitizeInt(bytes, 3);
                case 6:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitizeShort(bytes, 0))
                        * HASH_CODE_C2 + HashCodeAsciiSanitizeInt(bytes, 2);
                case 5:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitizsByte(bytes[0]))
                        * HASH_CODE_C2 + HashCodeAsciiSanitizeInt(bytes, 1);
                case 4:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitizeInt(bytes, 0);
                case 3:
                    return (hash * HASH_CODE_C1 + HashCodeAsciiSanitizsByte(bytes[0]))
                        * HASH_CODE_C2 + HashCodeAsciiSanitizeShort(bytes, 1);
                case 2:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitizeShort(bytes, 0);
                case 1:
                    return hash * HASH_CODE_C1 + HashCodeAsciiSanitizsByte(bytes[0]);
                default:
                    return hash;
            }
        }

        static int HashCodeAsciiCompute(ICharSequence value, int offset, int hash)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return hash * HASH_CODE_C1 +
                    // Low order int
                    HashCodeAsciiSanitizeInt(value, offset + 4) * HASH_CODE_C2 +
                    // High order int
                    HashCodeAsciiSanitizeInt(value, offset);
            }
            return hash * HASH_CODE_C1 +
                // Low order int
                HashCodeAsciiSanitizeInt(value, offset) * HASH_CODE_C2 +
                // High order int
                HashCodeAsciiSanitizeInt(value, offset + 4);
        }

        static int HashCodeAsciiSanitize(int value) => value & 0x1f1f1f1f;

        static int HashCodeAsciiSanitizeInt(ICharSequence value, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // mimic a unsafe.getInt call on a big endian machine
                return (value[offset + 3] & 0x1f)
                    | (value[offset + 2] & 0x1f) << 8
                    | (value[offset + 1] & 0x1f) << 16
                    | (value[offset] & 0x1f) << 24;
            }

            return (value[offset + 3] & 0x1f) << 24
                | (value[offset + 2] & 0x1f) << 16
                | (value[offset + 1] & 0x1f) << 8
                | (value[offset] & 0x1f);
        }

        static int HashCodeAsciiSanitizeShort(ICharSequence value, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // mimic a unsafe.getShort call on a big endian machine
                return (value[offset + 1] & 0x1f) 
                    | (value[offset] & 0x1f) << 8;
            }
            
            return (value[offset + 1] & 0x1f) << 8 
                | (value[offset] & 0x1f);
        }

        static int HashCodeAsciiSanitizsByte(char value) => value & 0x1f;
    }
}