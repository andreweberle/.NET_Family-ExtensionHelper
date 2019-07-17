using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;
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
                    bool customAttributeAdded = false;

                    var t = (T)Activator.CreateInstance(typeof(T));

                    System.Reflection.PropertyInfo[] propCollection = t.GetType().GetProperties();

                    foreach (System.Reflection.PropertyInfo propertyInfo in propCollection)
                    {
                        foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                        {
                            string attributeMapName = ((SqlPropertyName)attribute).MapName;

                            if (sqlDataReader[attributeMapName] != DBNull.Value)
                            {
                                propertyInfo.SetValue(t, sqlDataReader[attributeMapName]);
                                customAttributeAdded = true;
                            }
                        }

                        if (customAttributeAdded)
                        {
                            customAttributeAdded = false;
                            continue;
                        }

                        try
                        {
                            if (sqlDataReader.HasColumn(propertyInfo.Name) && sqlDataReader[propertyInfo.Name] != DBNull.Value)
                            {
                                propertyInfo.SetValue(t, sqlDataReader[propertyInfo.Name]);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("{0} Was Not A Found SQL Property", ex.Message));
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

        [AttributeUsage(AttributeTargets.Property)]
        public class SqlPropertyName : Attribute
        {
            public string MapName { get; private set; }
            public SqlPropertyName(string mapName) => MapName = mapName;
        }
    }
}