using System;

namespace DLaB.Xrm.LocalCrm
{
    internal static class PrimitiveTypeConverter
    {
        public static object ConvertToPrimitiveType(object value, Type? primitiveType)
        {
            if(primitiveType is null)
            {
                return value;
            }

            if (primitiveType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }
            if (primitiveType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            if (primitiveType == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            if (primitiveType == typeof(short))
            {
                return Convert.ToInt16(value);
            }
            if (primitiveType == typeof(byte))
            {
                return Convert.ToByte(value);
            }
            if (primitiveType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }
            if (primitiveType == typeof(double))
            {
                return Convert.ToDouble(value);
            }
            if (primitiveType == typeof(float))
            {
                return Convert.ToSingle(value);
            }
            if (primitiveType == typeof(char))
            {
                return Convert.ToChar(value);
            }
            if (primitiveType == typeof(string))
            {
                return Convert.ToString(value)!;
            }
            if (primitiveType == typeof(DateTime))
            {
                return Convert.ToDateTime(value);
            }
            if (primitiveType == typeof(uint))
            {
                return Convert.ToUInt32(value);
            }
            if (primitiveType == typeof(ulong))
            {
                return Convert.ToUInt64(value);
            }
            if (primitiveType == typeof(ushort))
            {
                return Convert.ToUInt16(value);
            }
            if (primitiveType == typeof(sbyte))
            {
                return Convert.ToSByte(value);
            }

            throw new InvalidOperationException($"Conversion to type {primitiveType} is not supported.");
        }
    }
}
