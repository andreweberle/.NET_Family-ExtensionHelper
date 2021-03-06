using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;
using System.Xml.Serialization;
using System.IO;
using EbbsSoft.ExtensionHelpers.EnumHelpers;
using EbbsSoft.ExtensionHelpers.StringHelpers;
using System.Reflection;
using System.Threading.Tasks;

namespace EbbsSoft.ExtensionHelpers.GenericHelpers
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
            // Get The System Exception.
            System.Exception ex = exception;

            // Start Returning Each Exception To The Caller.
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// Convert SqlDataReader Results To An Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlDataReader"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertToObject<T>(this SqlDataReader sqlDataReader) /* where T : class, new() */
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

                                // Break out of the loop.
                                break;
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
                // yield return null;
                yield return default(T);
            }
        }

        /// <summary>
        /// NameValueCollection To KeyValuePair
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> ToIEnumerable(this System.Collections.Specialized.NameValueCollection nvc)
        {
            foreach (string key in nvc.AllKeys)
            {
                yield return new KeyValuePair<string, string>(key, nvc[key]);
            }
        }

        /// <summary>
        /// Insert An Object To SQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool InsertInToDB<T>(this object obj, SqlConnection sqlConnection, string tableName) where T : class, new()
        {
            // Build Query
            System.Text.StringBuilder query = new System.Text.StringBuilder();

            // Get ColumnNames
            string[] columnName = sqlConnection.GetColumnNames(tableName).ToArray();

            // Get Query Started.
            query.Append($"INSERT INTO [{tableName}](");

            // Query Values.
            System.Reflection.PropertyInfo[] propertyInfos = obj.GetType().GetProperties();

            // We Want To Loop This Twice To Get The Column Name And Value.
            for (int i = 0; i != 2; i++)
            {
                // Help make more readable.
                bool isFirstLoop = i == 0;

                // Start looping the property info, we will check if there are custom attributes first as that will overrule and property names
                // as the user might have a different property name to what is in the table.
                // if i is zero, this means that we are only at the first half of building our query, this section is for the column names.
                foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
                {
                    // if the length is more than zero, there has been a custom attribute added.
                    if (propertyInfo.GetCustomAttributes(true).Length > 0)
                    {       
                        // One last check to see if the propertyname that was given is in the table.
                        // if it is not, we will attempt to add it in the else statement.
                        // this will use the original property name.
                        if (columnName.Any(x => x.ToLower() == ((SqlPropertyName)propertyInfo.GetCustomAttributes(true).First()).MapName.ToLower()))
                        {
                            // Conditional Statement would have been nice...damn
                            if (isFirstLoop)
                            {
                                query.Append($"[{((SqlPropertyName)propertyInfo.GetCustomAttributes(true).First()).MapName}],");
                            }
                            else
                            {
                                query.Append($"@{((SqlPropertyName)propertyInfo.GetCustomAttributes(true).First()).MapName.Replace(" ","")},");
                            }
                        }
                    }
                    else if (columnName.Any(x => x.ToLower() == propertyInfo.Name.ToLower()))
                    {
                        if (isFirstLoop)
                        {
                            query.Append($"[{propertyInfo.Name}],");
                        }
                        else
                        {
                            query.Append($"@{propertyInfo.Name.Replace(" ","")},");
                        }
                    }
                }

                if (i == 0)
                {
                    // Remove The Last Char.
                    query.Length--;

                    // Complete The Query.
                    query.Append(")VALUES(");
                }
                else
                {
                    // Remove The Last Char.
                    query.Length--;

                    // Complete The Query.
                    query.Append(")");
                }
            }

            // Here we will attempt to connect to the given database and execute the query.
            try
            {
                // Create The Command.
                using (SqlCommand sqlCommand = new SqlCommand(query.ToString(), sqlConnection))
                {
                    // Check If The Server Is Connected.
                    if (sqlConnection.ConnectedToServerAsync())
                    {
                        // Here we will attempt to create an sqlparameter use the db field type.
                        foreach ((System.Reflection.PropertyInfo propertyInfo, System.Data.SqlDbType sqlDataType) in from System.Reflection.PropertyInfo propertyInfo in propertyInfos
                                                                                                                     let sqlDataType = propertyInfo.PropertyType.TypeToSqlDbType()
                                                                                                                     select (propertyInfo, sqlDataType))
                        {
                            if (propertyInfo.GetCustomAttributes(true).Length > 0)
                            {
                                if (columnName.Any(x => x.ToLower() == ((SqlPropertyName)propertyInfo.GetCustomAttributes(true).First()).MapName.ToLower()))
                                {
                                    sqlCommand.Parameters.Add($"@{((SqlPropertyName)propertyInfo.GetCustomAttributes(true).First()).MapName.Replace(" ", "")}", sqlDataType).Value = propertyInfo.GetValue(obj, null) ?? DBNull.Value;
                                }
                                continue;
                            }
                            else if (columnName.Any(x => x.ToLower() == propertyInfo.Name.ToLower()))
                            {
                                sqlCommand.Parameters.Add($"@{propertyInfo.Name.Replace(" ","")}", sqlDataType).Value = propertyInfo.GetValue(obj, null) ?? DBNull.Value;
                                continue;
                            }
                        }

                        // Execute The Command And Return The Result.
                        return Convert.ToBoolean(sqlCommand.ExecuteNonQuery());
                    }
                    else
                    {
                        // Throw A Connection Error.
                        throw new Exception("Unable To Connect To Server.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Throw an error and all of the errros along with it.
                throw new Exception(string.Join(" ", ex.TryGetInnerExceptionsErrors().Select(x => x.Message)));
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        /// <summary>
        /// Get PropertyName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static object GetProperty<T>(this T obj, string propertyName)
        {
            // Check to see if the property name is correct.
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("message", nameof(propertyName));
            }

            try
            {        
                Type t = obj.GetType();
                PropertyInfo propertyInfo = t.GetProperty(propertyName);
                return propertyInfo.GetValue(obj);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Set Property Value
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static bool SetProperty<T>(this T obj, string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("message", nameof(propertyName));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            try
            {
                Type t = obj.GetType();
                PropertyInfo propertyInfo = t.GetProperty(propertyName);
                propertyInfo.SetValue(obj, value);
                return propertyInfo.GetValue(obj) == value;
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Fast Reverse
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> FastReverse<T>(this IList<T> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                yield return items[i];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool CompareObject<T>(this T obj1, T obj2)
        {
            if (obj1 is null || obj2 is null)
            {
                return false;
            }
            else
            {
                return obj1.ToJson() == obj2.ToJson();
            }
        }

        /// <summary>
        /// Clone Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(this T source)
        {
            // Convert To Json String.
            var serialized = source.ToJson();

            // Convert To T Object.
            return System.Text.Json.JsonSerializer.Deserialize<T>(serialized);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static async Task<T> FirstIfNotNullOrEmptyAsync<T>(this Task<IEnumerable<T>> obj) where T : class
        {
            IEnumerable<T> result = await obj;
            return result?.FirstOrDefault();
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
        public static T ConvertToXML<T>(this string data) where T : class, new()
        {
            try
            {
                // Get The Default Instance of The T Type.
                var t = Activator.CreateInstance(typeof(T));

                // Add String Data To The StringReader.
                using (var stringReader = new StringReader(data))
                {
                    // Setup The XmlSerializer
                    var serializer = new XmlSerializer(typeof(T));

                    // Attempt To Return The String Data In The Given T Type Object.
                    return (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                // Write The Error Response To The Debug Window.
                System.Diagnostics.Debug.WriteLine(string.Join(" ", ex.TryGetInnerExceptionsErrors().Select(x => x.Message)));

                // Return The Object With The Default Properties.
                return new T();
            }
        }
    }
}