using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DLaB.Common
{
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
                String[] fraction = config.Split('/');
                if (fraction.Length != 2)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Int32 numerator, denominator;

                if (Int32.TryParse(fraction[0], out numerator) && Int32.TryParse(fraction[1], out denominator))
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

            var parse = type.GetMethod("Parse", new[] {typeof (String)});

            if (parse == null)
            {
                value = (T) Convert.ChangeType(strValue, type);
            }
            else
            {
                value = (T) parse.Invoke(null, new Object[] {strValue});
            }
            return value;
        }
    }
}