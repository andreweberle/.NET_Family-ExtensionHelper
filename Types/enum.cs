using System;
using System.ComponentModel;

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
            SUCCESS = 0x000,
            ERROR = 0x0001,
            INFO = 0x0002
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
    }
}