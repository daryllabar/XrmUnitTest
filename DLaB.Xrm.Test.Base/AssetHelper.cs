using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xrm.Sdk;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest
#else

namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Contains Additional Asserts for Testing
    /// </summary>
    public static class AssertHelper
    {
        /// <summary>
        /// Checks if all attributes are equal between two entities, returning the attributes that are not matching.
        /// </summary>
        /// <param name="expected">The Entity with the expected attributes.</param>
        /// <param name="actual">The Entity with the actual attributes.</param>
        /// <returns></returns>
        public static string AttributesAreEqual(Entity expected, Entity actual)
        {
            if (expected == null && actual == null)
            {
                return null;
            }

            if (expected == null)
            {
                return "Expected null but actual was not null.";
            }

            if (actual == null)
            {
                return "Actual was null.";
            }

            foreach (var att in expected.Attributes)
            {
                if (actual.Contains(att.Key))
                {
                    var actualValue = actual[att.Key];
                    switch (att.Value)
                    {
                        case null when actualValue == null:
                            continue;
                        case null:
                            return $"Expected attribute: \"{att.Key}\" to be null, but actual was {actualValue.GetDisplayValue()}.";
                        default:
                            {
                                var value = AttributeIsEqual(actualValue, att);
                                if(value != null)
                                {
                                    return value;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    return $"Attribute {att.Key} was expected but not found!";
                }
            }
            return null;
        }

        private static string AttributeIsEqual(object actualValue, KeyValuePair<string, object> att)
        {
            if (actualValue == null)
            {
                return $"Expected attribute: \"{att.Key}\" with value: {att.Value.GetDisplayValue()} but actual was null.";
            }

            if (att.Value.Equals(actualValue))
            {
                return null;
            }

            if (!(actualValue is DateTime date))
            {
                return $"Expected attribute: \"{att.Key}\" with value: {att.Value.GetDisplayValue()} but actual was {actualValue.GetDisplayValue()}.";
            }

            // If hitting real database, and ff the actual time value is midnight, assume that the Date Time is a Date Only field, and compare dates only:
            if (!TestBase.UseLocalCrmDatabase
                && date == date.Date
                && att.Value is DateTime expectedDate
                && expectedDate.Date.Equals(date))
            {
                return null;
            }

            return $"Expected attribute: \"{att.Key}\" with value: {GetDateTimeTicks((DateTime)att.Value)} but actual was {GetDateTimeTicks(date)}.";

        }

        private static string GetDateTimeTicks(DateTime date)
        {
            return $"\"{date.ToShortDateString()} {date.ToLongTimeString()}\" ({date.Ticks})";
        }

        private static string GetDisplayValue(this object obj)
        {
            return obj switch
            {
                null => "null",
                Entity entity => entity.ToStringAttributes(),
                EntityReference entityRef => entityRef.ToStringDebug(),
                EntityCollection entities => entities.ToStringDebug(),
                EntityReferenceCollection entityRefCollection => entityRefCollection.ToStringDebug(),
                Dictionary<string, string> dict => dict.ToStringDebug(),
                byte[] imageArray => imageArray.ToStringDebug(),
                IEnumerable enumerable and not string => enumerable.ToStringDebug(),
                OptionSetValue optionSet => optionSet.Value.ToString(CultureInfo.InvariantCulture),
                Money money => money.Value.ToString(CultureInfo.InvariantCulture),
                bool yesNo => yesNo ? "true" : "false",
                _ => obj.IsNumeric() ? obj.ToString()! : $"\"{obj}\""
            };
        }

        private static bool IsNumeric(this object o)
        {
            return o is byte
                   || o is sbyte
                   || o is ushort
                   || o is uint
                   || o is ulong
                   || o is short
                   || o is int
                   || o is long
                   || o is float
                   || o is double
                   || o is decimal;
        }
    }
}
