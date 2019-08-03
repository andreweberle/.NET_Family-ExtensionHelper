using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace EbbsSoft.ExtensionHelpers.EnumHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Output Style Required For For PrintColouredText
        /// </summary>
        public enum OutputStyle
        {
            WRITE,
            WRITE_LINE
        }

        /// <summary>
        /// Encoding Types
        /// </summary>
        public enum EncodingFormat
        {
            ASCII,
            UT32,
            UTF8,
            UTF7,
            UNICODE
        }

        public enum PrintEventType
        {
            ERRO = 0x00,
            SUCCESS = 0x01,
            INFO = 0x02
        }

        /// <summary>
        /// Convert A String To An Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Get the Enum Value to String
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string EnumDescriptionToString(this Enum @enum)
        {
            if (@enum != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])@enum.GetType().GetField(@enum.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : string.Empty;
            }
            return "";
        }

        /// <summary>
        /// Convert Property Type To SqlDbType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Data.SqlDbType TypeToSqlDbType(this Type type)
        {
            // Build The Mappings.
            Dictionary<Type, System.Data.SqlDbType> mapType = new Dictionary<Type, System.Data.SqlDbType>
            {
                [typeof(string)] = SqlDbType.NVarChar,
                [typeof(char[])] = SqlDbType.NVarChar,
                [typeof(byte)] = SqlDbType.TinyInt,
                [typeof(short)] = SqlDbType.SmallInt,
                [typeof(int)] = SqlDbType.Int,
                [typeof(long)] = SqlDbType.BigInt,
                [typeof(byte[])] = SqlDbType.VarBinary,
                [typeof(bool)] = SqlDbType.Bit,
                [typeof(DateTime)] = SqlDbType.DateTime,
                [typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset,
                [typeof(decimal)] = SqlDbType.Money,
                [typeof(float)] = SqlDbType.Real,
                [typeof(double)] = SqlDbType.Float,
                [typeof(TimeSpan)] = SqlDbType.Time,
                [typeof(Guid)] = SqlDbType.UniqueIdentifier,
                [typeof(object)] = SqlDbType.Variant,
            };

            // Attempt To Get The Value.
            mapType.TryGetValue(type, out SqlDbType sqlDbType);

            // Return The Mapped Type To The Caller.
            return sqlDbType;
        }
    }
}