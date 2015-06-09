using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a switch statement has been defined for an Enum, but the actual value has not been defined as a case.
    /// Example:
    /// 	var c = BindingFlags.GetField;
    ///     try{
    ///     switch (c)
    ///     {
    ///        case BindingFlags.Default:
    ///        break;
    ///        default:
    ///        throw new EnumCaseUndefinedException&lt;BindingFlags&gt;(c, "Unable to perform reflection operation");
    ///     }
    /// </summary>
    [Serializable()]
    public class EnumCaseUndefinedException<TEnum> : Common.Exceptions.EnumCaseUndefinedException<TEnum> where TEnum : struct
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        public EnumCaseUndefinedException(TEnum undefinedEnumValue) : base(GetMessage(undefinedEnumValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        public EnumCaseUndefinedException(TEnum undefinedEnumValue, string message) : base(GetMessage(undefinedEnumValue, message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="inner">The inner exception.</param>
        public EnumCaseUndefinedException(TEnum undefinedEnumValue, Exception inner) : base(GetMessage(undefinedEnumValue), inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public EnumCaseUndefinedException(TEnum undefinedEnumValue, string message, Exception inner) : base(GetMessage(undefinedEnumValue, message), inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        public EnumCaseUndefinedException(int undefinedEnumValue) : base(GetMessage(undefinedEnumValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        public EnumCaseUndefinedException(int undefinedEnumValue, string message) : base(GetMessage(undefinedEnumValue, message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="inner">The inner.</param>
        public EnumCaseUndefinedException(int undefinedEnumValue, Exception inner) : base(GetMessage(undefinedEnumValue), inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public EnumCaseUndefinedException(int undefinedEnumValue, string message, Exception inner) : base(GetMessage(undefinedEnumValue, message), inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        public EnumCaseUndefinedException(OptionSetValue undefinedEnumValue) : base(GetMessage(undefinedEnumValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        public EnumCaseUndefinedException(OptionSetValue undefinedEnumValue, string message) : base(GetMessage(undefinedEnumValue, message)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="inner">The inner exception.</param>
        public EnumCaseUndefinedException(OptionSetValue undefinedEnumValue, Exception inner) : base(GetMessage(undefinedEnumValue), inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCaseUndefinedException{TEnum}"/> class.
        /// </summary>
        /// <param name="undefinedEnumValue">The undefined enum value.</param>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public EnumCaseUndefinedException(OptionSetValue undefinedEnumValue, string message, Exception inner) : base(GetMessage(undefinedEnumValue, message), inner) { }

        #endregion // Constructors

        private static string GetMessage(OptionSetValue undefinedEnumValue, string message = null)
        {
            var enumType = typeof(TEnum);
            if (!IsEnum(enumType, ref message)) { return message; }

            if (undefinedEnumValue == null)
            {
                return String.Format("{0}OptionSetValue was null for enum type {1}!", message, enumType.FullName);
            }

            return FormatMessage(message, enumType, (TEnum)(object)undefinedEnumValue.Value, undefinedEnumValue.Value);
        }
    }
}
