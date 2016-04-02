using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DLaB.Common
{
    /// <summary>
    /// Config Helper Class
    /// </summary>
    public class Config
    {
        #region Single Value GetAppSettings

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse to get the value.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetAppSettingOrDefault<T>(string appSetting, T defaultValue)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : config.ParseOrConvertString<T>();
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse to get the value.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the getDefault function will be used to retrieve the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static T GetAppSettingOrDefault<T>(string appSetting, Func<T> getDefault)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : config.ParseOrConvertString<T>();
        }

        /// <summary>
        /// Attempts to read the string setting from the config file, and convert it from a fraction to a decimal.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used
        /// </summary>
        /// <param name="appSetting"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal GetAppSettingFractionOrDefault(string appSetting, decimal defaultValue)
        {
            var config = ConfigurationManager.AppSettings[appSetting];

            if (config == null)
            {
                return defaultValue;
            }

            if (config.Contains('/'))
            {
                var fraction = config.Split('/');
                if (fraction.Length != 2)
                {
                    throw new ArgumentOutOfRangeException();
                }

                int numerator;
                int denominator;

                if (int.TryParse(fraction[0], out numerator) && int.TryParse(fraction[1], out denominator))
                {
                    if (denominator == 0)
                    {
                        throw new InvalidOperationException("Divide by 0 occurred");
                    }
                    return (decimal) numerator/denominator;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse to get the value.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The setting should be in the format {Key}:{Value}|{Key}:{Value}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="key">AppSetting Key attribute value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static T GetAppSettingOrDefaultByKey<T>(string appSetting, string key, T defaultValue, ConfigKeyValueSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            if (config == null)
            {
                return defaultValue;
            }

            info = info ?? ConfigKeyValueSplitInfo.Default;

            // Find the Key:Value where the Key Matches
            config = config.Split(info.EntrySeperators).
                FirstOrDefault(v => v.Split(info.KeyValueSeperators).
                                      Select(k => info.ParseKey<string>(k)).
                                      FirstOrDefault() == info.ParseKey<string>(key));
            return config == null ? defaultValue : SubstringByString(config, info.KeyValueSeperators.ToString()).ParseOrConvertString<T>();
        }

        /// <summary>
        /// Returns a the substring after the index of the first occurence of the startstring.
        /// Example: "012345678910".SubstringByString("2"); returns "345678910"
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="startString">The string that marks the start of the substring to be returned.</param>
        /// <returns></returns>
        private static string SubstringByString(string value, string startString)
        {
            var start = value.IndexOf(startString, StringComparison.Ordinal);
            return start < 0 ? null : value.Substring(start + startString.Length);
        }

        #endregion Single Value GetAppSettings

        #region GetList

        /// <summary>
        /// Attempts to read the setting from the config file and Parse to get the value.
        /// The value from the config if first split by the seperator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting"></param>
        /// <param name="defaultValue"></param>
        /// <param name="seperator">If null, ',' is used</param>
        /// <returns></returns>
        public static List<T> GetAppSettingListOrDefault<T>(string appSetting, List<T> defaultValue, char[] seperator = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];

            if (config == null)
            {
                return defaultValue;
            }

            var value = new List<T>();
            var configs = config.Split(seperator ?? new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!configs.Any())
            {
                value = defaultValue;
            }
            else if (typeof(T).IsEnum)
            {
                value.AddRange(configs.Select(Extensions.ParseOrConvertString<T>));
            }

            return value;
        }

        #endregion GetList

        #region GetDictionary

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value}|{Key}:{Value}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="getDefault">Function to get the default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting,
                                                                           Func<Dictionary<TKey, TValue>> getDefault,
                                                                           ConfigKeyValueSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : GetDictionary<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value}|{Key}:{Value}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting, 
                                                                           Dictionary<TKey, TValue> defaultValue,
                                                                           ConfigKeyValueSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : GetDictionary<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value}|{Key}:{Value}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting,
                                                                           string defaultValue,
                                                                           ConfigKeyValueSplitInfo info = null)
        {
            return GetDictionary<TKey, TValue>(info, ConfigurationManager.AppSettings[appSetting] ?? defaultValue);
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(ConfigKeyValueSplitInfo info, string config)
        {
            info = info ?? ConfigKeyValueSplitInfo.Default;

            return config.Split(info.EntrySeperators, StringSplitOptions.RemoveEmptyEntries).
                          Select(entry => entry.Split(info.KeyValueSeperators, StringSplitOptions.RemoveEmptyEntries)).
                          ToDictionary(values => info.ParseKey<TKey>(values[0]),
                              values => info.ParseValue<TValue>(values.Length > 1 ? values[1] : null));
        }

        #endregion GetDictionary

        #region GetDictionaryList

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(string appSetting,
                                                                                     Dictionary<TKey, List<TValue>> defaultValue,
                                                                                     ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : GetDictionaryList<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(string appSetting,
                                                                                     string defaultValue,
                                                                                     ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting] ?? defaultValue;
            return GetDictionaryList<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="getDefault">Function to get the default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(string appSetting,
                                                                                     Func<Dictionary<TKey, List<TValue>>> getDefault,
                                                                                     ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : GetDictionaryList<TKey, TValue>(info, config);
        }

        private static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(ConfigKeyValuesSplitInfo info, string config)
        {
            info = info ?? ConfigKeyValuesSplitInfo.Default;
            var dict = new Dictionary<TKey, List<TValue>>();
            foreach (var entry in config.Split(info.EntrySeperators, StringSplitOptions.RemoveEmptyEntries))
            {
                var entryValues = entry.Split(info.KeyValueSeperators, StringSplitOptions.RemoveEmptyEntries);
                var value = entryValues.Length > 1
                    ? entryValues[1].Split(info.EntryValuesSeperators, StringSplitOptions.RemoveEmptyEntries).Select(info.ParseValue<TValue>).ToList()
                    : new List<TValue>();
                dict.Add(info.ParseKey<TKey>(entryValues[0]), value);
            }

            return dict;
        }

        #endregion GetDictionaryList

        #region GetDictionaryHash

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, HashSet<TValue>> GetDictionaryHash<TKey, TValue>(string appSetting,
                                                                                     Dictionary<TKey, HashSet<TValue>> defaultValue,
                                                                                     ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : GetDictionaryHash<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, HashSet<TValue>> GetDictionaryHash<TKey, TValue>(string appSetting,
                                                                                        string defaultValue,
                                                                                        ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting] ?? defaultValue;
            return GetDictionaryHash<TKey, TValue>(info, config);
        }

        /// <summary>
        /// Attempts to read the setting from the config file, and Parse into a Dictionary.
        /// If the Type doesn't contain a Parse, a cast is attempted.
        /// Any failure in the Parse will throw an exception.
        /// If the config value is null, then the default value will be used.
        /// The default setting should be in the format {Key}:{Value1},{value2}|{Key}:{Value1}
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="appSetting">The application setting.</param>
        /// <param name="getDefault">Function to get the default value.</param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static Dictionary<TKey, HashSet<TValue>> GetDictionaryHash<TKey, TValue>(string appSetting,
                                                                                        Func<Dictionary<TKey, HashSet<TValue>>> getDefault,
                                                                                        ConfigKeyValuesSplitInfo info = null)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : GetDictionaryHash<TKey, TValue>(info, config);
        }

        private static Dictionary<TKey, HashSet<TValue>> GetDictionaryHash<TKey, TValue>(ConfigKeyValuesSplitInfo info, string config)
        {
            info = info ?? ConfigKeyValuesSplitInfo.Default;
            var dict = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var entry in config.Split(info.EntrySeperators, StringSplitOptions.RemoveEmptyEntries))
            {
                var entryValues = entry.Split(info.KeyValueSeperators, StringSplitOptions.RemoveEmptyEntries);
                var value = entryValues.Length > 1
                    ? new HashSet<TValue>(entryValues[1].Split(info.EntryValuesSeperators, StringSplitOptions.RemoveEmptyEntries).Select(info.ParseValue<TValue>))
                    : new HashSet<TValue>();
                dict.Add(info.ParseKey<TKey>(entryValues[0]), value);
            }

            return dict;
        }

        #endregion GetDictionaryHash

        #region GetHashSet

        /// <summary>
        /// Config Value must be in the format "Value|Value|Value" by Default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting"></param>
        /// <param name="info">The settings by which to split the config value.</param>
        /// <returns></returns>
        public static HashSet<T> GetHashSet<T>(string appSetting, ConfigKeyValueSplitInfo info = null)
        {
            var value = new HashSet<T>();
            var config = ConfigurationManager.AppSettings[appSetting];
            if (config != null)
            {
                info = info ?? ConfigKeyValueSplitInfo.Default;
                value = new HashSet<T>(config.Split(info.EntrySeperators).Select(info.ParseKey<T>));
            }
            return value;
        }

        #endregion GetHashSet
    }
}