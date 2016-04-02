namespace DLaB.Common
{
    /// <summary>
    /// Settings used to control how a Key/Value config setting is parsed.  
    /// </summary>
    public class ConfigKeyValueSplitInfo
    {
        private static ConfigKeyValueSplitInfo @default = new ConfigKeyValueSplitInfo();
        internal static ConfigKeyValueSplitInfo Default { get { return @default; } }

        /// <summary>
        /// The Default Entry Seperator
        /// </summary>
        public const char Entry_Seperator = '|';
        /// <summary>
        /// The Default Key/Value Seperator
        /// </summary>
        public const char KeyValue_Seperator = ':';

        /// <summary>
        /// Gets or sets the entry seperators.
        /// </summary>
        /// <value>
        /// The entry seperators.
        /// </value>
        public char[] EntrySeperators { get; set; }
        /// <summary>
        /// Gets or sets the key value seperators.
        /// </summary>
        /// <value>
        /// The key value seperators.
        /// </value>
        public char[] KeyValueSeperators { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [convert keys to lower].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [convert keys to lower]; otherwise, <c>false</c>.
        /// </value>
        public bool ConvertKeysToLower { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [convert values to lower].
        /// </summary>
        /// <value>
        /// <c>true</c> if [convert values to lower]; otherwise, <c>false</c>.
        /// </value>
        public bool ConvertValuesToLower { get; set; }

        /// <summary>
        /// Defaults to splitting entries by "|", and Key/values by ":", lower casing the keys.<para />
        /// For Example:<para />
        /// - Entry1Key:Entry1Value|Entry2Key:Entry2Value|Entry3Key:Entry3Value|Entry4Key:Entry4Value
        /// </summary>
        public ConfigKeyValueSplitInfo()
        {
            EntrySeperators = new[] { Entry_Seperator };
            KeyValueSeperators = new[] { KeyValue_Seperator };
            ConvertKeysToLower = true;
            ConvertValuesToLower = false;
        }

        internal T ParseKey<T>(string key)
        {
            if(key == null)
            {
                return default(T);
            }
            return (ConvertKeysToLower ? key.ToLower() : key).ParseOrConvertString<T>();
        }

        internal T ParseValue<T>(string value)
        {
            if (value == null)
            {
                return default(T);
            }
            return (ConvertValuesToLower ? value.ToLower() : value).ParseOrConvertString<T>();
        }
    }
}
