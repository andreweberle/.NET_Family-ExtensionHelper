using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;
using System.Xml.Serialization;
using System.IO;
using EbbsSoft.ExtensionHelpers.StringHelpers;

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
        /// Get A Colour From Html Code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetColourFromHtmlCode(this string code) => System.Drawing.ColorTranslator.FromHtml(code);

        /// <summary>
        /// Convert SqlDataReader Results To An Object.
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
                    // This is so if there is a custom attribute assigned to a property
                    // we will know if we have to skip to the next property
                    // as we will assign the custom attributes first.
                    bool customAttributeAdded = false;

                    // Create an instance of the T type,
                    // T can be anything really, so we need this in order to get the
                    // defaults of the class that is given.
                    var t = (T)Activator.CreateInstance(typeof(T));

                    // Get all property Information in the T object.
                    System.Reflection.PropertyInfo[] propCollection = t.GetType().GetProperties();

                    // Start looping each propertyinfo object in propCollection.
                    foreach (System.Reflection.PropertyInfo propertyInfo in propCollection)
                    {
                        // Start looping each custom attribute if any in the properyinfo object.
                        foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                        {
                            // Get The name.
                            string attributeMapName = ((SqlPropertyName)attribute).MapName;

                            // Check if the data is not null from the reader.
                            // if it is null, we will skip the the next item,
                            // as we dont want it, the object will already be the
                            // default type.
                            if (sqlDataReader[attributeMapName] != DBNull.Value)
                            {
                                // Set the value here,
                                // This will add the value to the object that we will return
                                // T
                                propertyInfo.SetValue(t, sqlDataReader[attributeMapName]);

                                // As a custom attribute was added,
                                // we will use this to continue on outside of all loops.
                                customAttributeAdded = true;
                            }
                        }

                        // Check if a custom attribute was added.
                        if (customAttributeAdded)
                        {
                            // Reset the boolean and go to the next property.
                            customAttributeAdded = false;
                            continue;
                        }

                        try
                        {
                            // At this point, we will attempt to assign the propery object
                            // this will require that the sqlDataReader has the given properyName as a column
                            // and that the value from the reader is not null.
                            if (sqlDataReader.HasColumn(propertyInfo.Name) && sqlDataReader[propertyInfo.Name] != DBNull.Value)
                            {
                                propertyInfo.SetValue(t, sqlDataReader[propertyInfo.Name]);
                            }
                        }
                        catch (Exception ex)
                        {
                            // throw an error as there was no custom attribute and the property name was
                            // not a valid column name.
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

        /// <summary>
        /// SqlProperty Name
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class SqlPropertyName : Attribute
        {
            /// <summary>
            /// Map The Name.
            /// </summary>
            /// <value></value>
            public string MapName { get; private set; }
            
            /// <summary>
            /// Sql
            /// </summary>
            /// <param name="mapName"></param>
            public SqlPropertyName(string mapName)
            {
                if (mapName is null)
                {
                    throw new ArgumentNullException(nameof(mapName));
                }
                else
                {
                    this.MapName = mapName;
                }
            }
        }

        /// <summary>
        /// Convert XML String To Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T ConvertToXML<T>(this string data) where T :class, new()
        {
            try
            {     
                var t = Activator.CreateInstance(typeof(T));            
                using (var stringReader = new StringReader(data))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Join(" ", ex.TryGetInnerExceptionsErrors().Select(x => x.Message)));
                return new T();
            }
        }
    }
}