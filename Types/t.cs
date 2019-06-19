using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EbbsSoft.ExtensionHelpers.T_Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Remove Null Items From A List.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> RemoveNullItemsToList<T>(this List<T> list)
        {
            return list.Where(x => x != null).ToList();
        }

        // public static List<T> RemoveBlankItemsToList<T>(this List<T> list)
        // {
        //     return list.Where(x => x)
        // }
    }
}