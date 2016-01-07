using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [Serializable]
    public struct KeyValuePair2<TKey, TValue>
    {
        private TKey key;
        private TValue value;

        /// <summary>
        /// Gets the key in the key/value pair.
        /// </summary>
        /// 
        /// <returns>
        /// A <paramref name="TKey"/> that is the key of the <see cref="T:System.Collections.Generic.KeyValuePair`2"/>.
        /// </returns>
        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        /// <summary>
        /// Gets the value in the key/value pair.
        /// </summary>
        /// 
        /// <returns>
        /// A <paramref name="TValue"/> that is the value of the <see cref="T:System.Collections.Generic.KeyValuePair`2"/>.
        /// </returns>
        public TValue Value
        {
            get
            {
                return this.value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.KeyValuePair`2"/> structure with the specified key and value.
        /// </summary>
        /// <param name="key">The object defined in each key/value pair.</param><param name="value">The definition associated with <paramref name="key"/>.</param>
        public KeyValuePair2(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="T:System.Collections.Generic.KeyValuePair`2"/>, using the string representations of the key and value.
        /// </summary>
        /// 
        /// <returns>
        /// A string representation of the <see cref="T:System.Collections.Generic.KeyValuePair`2"/>, which includes the string representations of the key and value.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = StringBuilderCache.Acquire(16);
            sb.Append('[');
            if ((object)this.Key != null)
                sb.Append(this.Key.ToString());
            sb.Append(", ");
            if ((object)this.Value != null)
                sb.Append(this.Value.ToString());
            sb.Append(']');
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        internal static class StringBuilderCache
        {
            private const int MAX_BUILDER_SIZE = 360;
            [ThreadStatic]
            private static StringBuilder CachedInstance;

            public static StringBuilder Acquire(int capacity = 16)
            {
                if (capacity <= 360)
                {
                    StringBuilder stringBuilder = StringBuilderCache.CachedInstance;
                    if (stringBuilder != null && capacity <= stringBuilder.Capacity)
                    {
                        StringBuilderCache.CachedInstance = (StringBuilder)null;
                        stringBuilder.Clear();
                        return stringBuilder;
                    }
                }
                return new StringBuilder(capacity);
            }

            public static void Release(StringBuilder sb)
            {
                if (sb.Capacity > 360)
                    return;
                StringBuilderCache.CachedInstance = sb;
            }

            public static string GetStringAndRelease(StringBuilder sb)
            {
                string str = sb.ToString();
                StringBuilderCache.Release(sb);
                return str;
            }
        }
    }
}