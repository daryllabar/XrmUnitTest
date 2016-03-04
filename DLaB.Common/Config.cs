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
            return config == null ? defaultValue : ParseOrConvertString<T>(config);
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
            return config == null ? getDefault() : ParseOrConvertString<T>(config);
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
        /// <param name="appSetting"></param>
        /// <param name="key">AppSetting Key attribute value.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetAppSettingOrDefaultByKey<T>(string appSetting, string key, T defaultValue)
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            if (config == null)
            {
                return defaultValue;
            }

            // Find the Key:Value where the Key Matches
            config = config.Split('|').FirstOrDefault(v => v.Split(':').FirstOrDefault() == key);
            return config == null ? defaultValue : ParseOrConvertString<T>(SubstringByString(config, ":"));
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
            var value = new List<T>();
            var config = ConfigurationManager.AppSettings[appSetting];

            if (config == null)
            {
                value = defaultValue;
            }
            else
            {
                seperator = seperator ?? new[] {','};

                var configs = config.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                if (!configs.Any())
                {
                    value = defaultValue;
                }
                else if (typeof (T).IsEnum)
                {
                    // Allow T to be an Enum, parse as string, then cast to T
                    value.AddRange(configs.Select(ParseOrConvertString<int>).Cast<T>());
                }
                else
                {
                    value.AddRange(configs.Select(ParseOrConvertString<T>));
                }

            }

            return value;
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
        /// <param name="getDefault">Function to get the default value.</param>
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting,
                                                                           Func<Dictionary<TKey, TValue>> getDefault,
                                                                           char entrySeperator = '|',
                                                                           char keyValueSeperator = ':')
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : GetDictionary<TKey, TValue>(entrySeperator, keyValueSeperator, config);
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
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting, 
                                                                           Dictionary<TKey, TValue> defaultValue, 
                                                                           char entrySeperator = '|',
                                                                           char keyValueSeperator = ':')
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : GetDictionary<TKey, TValue>(entrySeperator, keyValueSeperator, config);
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
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string appSetting,
                                                                           string defaultValue,
                                                                           char entrySeperator = '|',
                                                                           char keyValueSeperator = ':')
        {
            return GetDictionary<TKey, TValue>(entrySeperator, keyValueSeperator, ConfigurationManager.AppSettings[appSetting] ?? defaultValue);
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(char entrySeperator, char keyValueSeperator, string config)
        {
            return config.Split(new[] {entrySeperator}, StringSplitOptions.RemoveEmptyEntries).
                          Select(entry => entry.Split(new[] {keyValueSeperator}, StringSplitOptions.RemoveEmptyEntries)).
                          ToDictionary(values => ParseOrConvertString<TKey>(values[0]),
                              values => ParseOrConvertString<TValue>(values.Length > 1 ? values[1] : null));
        }

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
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <param name="entryValueSeperator">The entry value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(string appSetting,
                                                                                     Dictionary<TKey, List<TValue>> defaultValue,
                                                                                     char entrySeperator = '|',
                                                                                     char keyValueSeperator = ':',
                                                                                     char entryValueSeperator = ',')
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? defaultValue : GetDictionaryList<TKey, TValue>(entrySeperator, keyValueSeperator, entryValueSeperator, config);
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
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <param name="entryValueSeperator">The entry value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(
            string appSetting,
            string defaultValue,
            char entrySeperator = '|',
            char keyValueSeperator = ':',
            char entryValueSeperator = ',')
        {
            var config = ConfigurationManager.AppSettings[appSetting] ?? defaultValue;
            return GetDictionaryList<TKey, TValue>(entrySeperator, keyValueSeperator, entryValueSeperator, config);
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
        /// <param name="entrySeperator">The entry seperator.</param>
        /// <param name="keyValueSeperator">The key value seperator.</param>
        /// <param name="entryValueSeperator">The entry value seperator.</param>
        /// <returns></returns>
        public static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(
            string appSetting,
            Func<Dictionary<TKey, List<TValue>>> getDefault,
            char entrySeperator = '|',
            char keyValueSeperator = ':',
            char entryValueSeperator = ',')
        {
            var config = ConfigurationManager.AppSettings[appSetting];
            return config == null ? getDefault() : GetDictionaryList<TKey, TValue>(entrySeperator, keyValueSeperator, entryValueSeperator, config);
        }

        private static Dictionary<TKey, List<TValue>> GetDictionaryList<TKey, TValue>(char entrySeperator, char keyValueSeperator, char entryValueSeperator, string config)
        {
            var dict = new Dictionary<TKey, List<TValue>>();
            foreach (var entry in config.Split(new[] {entrySeperator}, StringSplitOptions.RemoveEmptyEntries))
            {
                var entryValues = entry.Split(new[] {keyValueSeperator}, StringSplitOptions.RemoveEmptyEntries);
                var value = entryValues.Length > 1
                    ? entryValues[1].Split(new[] {entryValueSeperator}, StringSplitOptions.RemoveEmptyEntries).Select(ParseOrConvertString<TValue>).ToList()
                    : new List<TValue>();
                dict.Add(ParseOrConvertString<TKey>(entryValues[0]), value);
            }

            return dict;
        }

        #endregion GetDictionaryList

        /// <summary>
        /// Config Value must be in the format "Value|Value|Value"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSetting"></param>
        /// <returns></returns>
        public static HashSet<T> GetHashSet<T>(string appSetting)
        {
            var value = new HashSet<T>();
            var config = ConfigurationManager.AppSettings[appSetting];
            if (config != null)
            {
                value = new HashSet<T>(config.Split('|').Select(ParseOrConvertString<T>));
            }
            return value;
        }

        private static T ParseOrConvertString<T>(string strValue)
        {
            T value;
            var type = typeof (T);

            var parse = type.GetMethod("Parse", new[] {typeof (string) });
            try
            {
                if (parse == null)
                {
                    value = (T) Convert.ChangeType(strValue, type);
                }
                else
                {
                    value = (T) parse.Invoke(null, new object[] {strValue});
                }
            }
            catch (Exception)
            {
                throw new FormatException($"Unable to convert \"{strValue}\" into type {typeof(T).FullName}.");
            }
            return value;
        }
    }
}