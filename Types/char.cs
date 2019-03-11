using System;
using System.Collections.Generic;

namespace EbbsSoft.ExtensionHelpers.CharHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Returns the first Recurive Char of a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char? RecuriveChar(this string str)
        {
            // Holds the results, key will be the char and string will be the number
            // of times we have seen it.
            Dictionary<char, string> keyValuePairs = new Dictionary<char, string>();

            foreach (char c in str)
            {
                if (keyValuePairs.ContainsKey(c))
                {
                    return c;
                }
                else
                {
                    keyValuePairs.Add(c, "1");
                }
            }

            return null;
        }
    }
}