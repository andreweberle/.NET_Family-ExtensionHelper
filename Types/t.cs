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
        public static List<T> RemoveNullItemsToList<T>(this List<T> list) => list.Where(x => x != null).ToList();

        /// <summary>
        /// Attempt To Read All Innter Exceptions From The Hierarchy
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static IEnumerable<Exception> TryGetInnerExceptionsErrors(this Exception exception)
        {
            System.Exception ex = exception;

            while (ex != null)         
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }
    }
}