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
        
        /// <summary>
        /// Convert SqlDataReader Results To Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlDataReader"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertToObject<T>(this SqlDataReader sqlDataReader) where T : class, new()
        {
            // Check that the data reader has data to begin with.
            if (sqlDataReader.HasRows)
            {
                // Start reading each value.
                while (sqlDataReader.Read())
                {
                    // We will start by converting T into the given return object.
                    var t = (T)Activator.CreateInstance(typeof(T));

                    // Start looping each property
                    // we will use the propertyinfo's name to match the T property
                    // once that is done, we will be able to set the properties value.
                    foreach (System.Reflection.PropertyInfo propertyInfo in t.GetType().GetProperties())
                    {
                        // We dont wanna add empty properties,
                        // are can remain what ever it is that their null property is.
                        if (sqlDataReader[propertyInfo.Name] != DBNull.Value)
                        {
                            // Set the value here.
                            propertyInfo.SetValue(t, sqlDataReader[propertyInfo.Name]);
                        }
                    }

                    // return the object back to the caller,
                    // start loop again if needed.
                    yield return t;
                }
            }
            else
            {
                // nothing was found.
                yield return null;
            }
        }
    }
}