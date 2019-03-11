using System;
using System.Linq;
using System.Text;
using EbbsSoft.ExtensionHelpers.StringHelpers;

namespace EbbsSoft.ExtensionHelpers.IntegerHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Reverse a Number Set.
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static int Reverse(this int[] numbers)
        {
            StringBuilder sb = new StringBuilder();
            Array.ForEach(numbers, x => sb.Append(x));
            return Convert.ToInt32(EbbsSoft.ExtensionHelpers.StringHelpers.Utils.Reverse(sb.ToString()));
        }

        /// <summary>
        /// Generate a true random From 0 to (n)
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetTrueRandom(this int max)
        {
            // Sync Lock Object.
            object syncLock = new object();

            // Get Random Number Object With Guid Seed As Seed.
            Random random = new Random(Guid.NewGuid().GetHashCode());

            // Lock the Sync Lock Object
            // So Hopefully No Duplicates Happen.
            lock (syncLock)
            {
                // Return Results To The Caller.
                return random.Next(0, max);
            }
        }

        /// <summary>
        /// Return the number of bytes
        /// for a string object.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="encodingFormat"></param>
        /// <returns></returns>
        public static int? GetBytesCount(this string @string, EnumHelpers.Utils.EncodingFormat encodingFormat = EnumHelpers.Utils.EncodingFormat.ASCII)
        {
            switch (encodingFormat)
            {
                case EnumHelpers.Utils.EncodingFormat.ASCII:
                    return System.Text.Encoding.ASCII.GetByteCount(@string);

                case EnumHelpers.Utils.EncodingFormat.UTF7:
                    return System.Text.Encoding.UTF7.GetByteCount(@string);

                case EnumHelpers.Utils.EncodingFormat.UTF8:
                    return System.Text.Encoding.UTF8.GetByteCount(@string);

                case EnumHelpers.Utils.EncodingFormat.UT32:
                    return System.Text.Encoding.UTF32.GetByteCount(@string);

                case EnumHelpers.Utils.EncodingFormat.UNICODE:
                    return System.Text.Encoding.Unicode.GetByteCount(@string);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Counts the number of words in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int WordCount(this string str)
        {
            string[] words = null;

            if (str.Contains(" "))
            {
                words = str.Split(' ');
            }

            return words.Count();
        }

        /// <summary>
        /// Get the age from DateTime
        /// </summary>
        /// <param name="dateTime">Birthday</param>
        /// <returns></returns>
        public static int AgeFromDateTime(this DateTime dateTime)
        {
            // Todays Date
            DateTime today = DateTime.Today;
            // Get The Age
            int age = today.Year - dateTime.Year;
            // Check For Leap Year.
            if (dateTime > today.AddYears(-age))
            {
                age--;
            }
            // Return The Age From DateTime.
            return age;
        }
    }
}