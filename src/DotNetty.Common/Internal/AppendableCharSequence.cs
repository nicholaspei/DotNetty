// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using DotNetty.Common.Utilities;

    public sealed class AppendableCharSequence : ICharSequence, IAppendable, IEquatable<AppendableCharSequence>
    {
        char[] chars;
        int pos;

        public AppendableCharSequence(int length)
        {
            Contract.Requires(length > 0);

            this.chars = new char[length];
        }

        AppendableCharSequence(char[] chars)
        {
            Contract.Requires(chars.Length > 0);

            this.chars = chars;
            this.pos = chars.Length;
        }

        public IEnumerator<char> GetEnumerator()
        {
            foreach (char value in this.chars)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.pos;

        public char this[int index]
        {
            get
            {
                Contract.Requires(index <= this.pos);
                return this.chars[index];
            }
        }

        /**
          * Access a value in this {@link CharSequence}.
          * This method is considered unsafe as index values are assumed to be legitimate.
          * Only underlying array bounds checking is done.
          * @param index The index to access the underlying array at.
          * @return The value at {@code index}.
          */
        public char CharAtUnsafe(int index) => this.chars[index];

        public ICharSequence SubSequence(int start, int end)
        {
            int length = end - start;
            var data = new char[length];
            Array.Copy(this.chars, start, data, 0, length);

            return new AppendableCharSequence(data);
        }

        public int IndexOf(char ch, int start = 0) => StringUtil.IndexOf(this.chars, ch, start);

        public bool RegionMatches(bool ignoreCase, int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatches(this, ignoreCase, thisStart, seq, start, length);

        public bool RegionMatches(int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatches(this, thisStart, seq, start, length);

        public bool Equals(AppendableCharSequence other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other) 
                || ReferenceEquals(this.chars, other.chars) && this.pos == other.pos)
            {
                return true;
            }

            return this.SequenceEquals(other, false);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, null))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as AppendableCharSequence;
            if (other != null)
            {
                return this.Equals(other);
            }

            var sequence = obj as ICharSequence;
            if (sequence != null)
            {
                return this.SequenceEquals(sequence, false);
            }

            return false;
        }

        public int HashCode(bool ignoreCase) => ignoreCase
            ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.ToString())
            : StringComparer.Ordinal.GetHashCode(this.ToString());

        public override int GetHashCode() => this.HashCode(true);

        public bool SequenceEquals(ICharSequence other, bool ignoreCase) =>
            CharUtil.SequenceEquals(this, other, ignoreCase);

        public IAppendable Append(char c)
        {
            try
            {
                this.chars[this.pos++] = c;
            }
            catch (IndexOutOfRangeException)
            {
                this.Expand();
                this.chars[this.pos - 1] = c;
            }

            return this;
        }

        public IAppendable Append(ICharSequence sequence) => this.Append(sequence, 0, sequence.Count);

        public IAppendable Append(ICharSequence sequence, int start, int end)
        {
            Contract.Requires(sequence.Count >= end);

            int length = end - start;
            if (length > this.chars.Length - this.pos)
            {
                this.chars = Expand(this.chars, this.pos + length, this.pos);
            }

            var seq = sequence as AppendableCharSequence;
            if (seq != null)
            {
                // Optimize append operations via array copy
                char[] src = seq.chars;
                Buffer.BlockCopy(src, start, this.chars, this.pos, length);
                this.pos += length;

                return this;
            }

            for (int i = start; i < end; i++)
            {
                this.chars[this.pos++] = sequence[i];
            }

            return this;
        }


        /**
          * Reset the {@link AppendableCharSequence}. Be aware this will only reset the current internal position and not
          * shrink the internal char array.
          */
        public void Reset() => this.pos = 0;

        public string ToString(int start)
        {
            Contract.Requires(start >= 0 && start < this.Count);

            return new string(this.chars, start, this.pos);
        }

        public override string ToString() => this.Count == 0 ? string.Empty : this.ToString(0);

        /**
          * Create a new {@link String} from the given start to end.
          * This method is considered unsafe as index values are assumed to be legitimate.
          * Only underlying array bounds checking is done.
         */
        public string SubStringUnsafe(int start, int end) => new string(this.chars, start, end - start);

        void Expand()
        {
            char[] old = this.chars;
            // double it
            int len = old.Length << 1;
            if (len < 0)
            {
                throw new InvalidOperationException($"Length {len} must be positive");
            }

            this.chars = new char[len];
            Buffer.BlockCopy(old, 0, this.chars, 0, old.Length);
        }

        static char[] Expand(char[] array, int neededSpace, int size)
        {
            int newCapacity = array.Length;
            do
            {
                // double capacity until it is big enough
                newCapacity <<= 1;

                if (newCapacity < 0)
                {
                    throw new InvalidOperationException($"New capacity {newCapacity} must be positive");
                }

            } while (neededSpace > newCapacity);

            var newArray = new char[newCapacity];
            Buffer.BlockCopy(array, 0, newArray, 0, size);

            return newArray;
        }
    }
}
