using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;
using ZetaLongPaths;
using static EbbsSoft.ExtensionHelpers.Overrides.Utils;

namespace EbbsSoft.ExtensionHelpers.StringHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Returns a particular value from an Xml Document
        /// object type.
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="elementTagName"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string TryReadValueFromXmlDoc(this XmlDocument xmlDocument, string elementTagName, string attribute)
        {
            return xmlDocument.GetElementsByTagName(elementTagName).Cast<XmlNode>()
                                                                   .Select(x => x.Attributes[attribute].Value).First();  
        }

        /// <summary>
        /// Returns a particular value from an xml document
        /// using an xpath eg "//Person/FirstName"
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="xPath"></param>
        /// <returns></returns>
        public static string TryReadValueFromXmlDoc(this XmlDocument xmlDocument, string xPath)
        {
            return xmlDocument.SelectSingleNode(xPath).InnerXml;
        }

        /// <summary>
        /// Convert A Number To
        /// Readable English
        /// https://stackoverflow.com/questions/554314/how-can-i-convert-an-integer-into-its-verbal-representation
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToWords(this int number)
        {
            if (number == 0)
            {
                return "zero";
            }

            if (number < 0)
            {
                return "minus " + ToWords(Math.Abs(number));
            }

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += ToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                {
                    words += "and ";
                }

                string[] unitsMap = new string[] 
                {
                    "zero","one",
                    "two","three",
                    "four","five",
                    "six","seven",
                    "eight","nine",
                    "ten","eleven",
                    "twelve","thirteen",
                    "fourteen","fifteen",
                    "sixteen","seventeen",
                    "eighteen","nineteen"
                };
                string[] tensMap = new string[] 
                {
                    "zero", "ten",
                    "twenty", "thirty",
                    "forty", "fifty",
                    "sixty", "seventy",
                    "eighty", "ninety"
                };

                if (number < 20)
                {
                    words += unitsMap[number];
                }
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += "-" + unitsMap[number % 10];
                    }
                }
            }
            return words;
        }

        /// <summary>
        /// Get the Assembly File Version.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static string AssemblyVersion(this string assemblyPath)
        {
            if (assemblyPath.IsValidFilePath())
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an Object type (if valid) into a 
        /// readable size value
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToHumanReadableSize(this object bytes)
        {
            // https://msdn.microsoft.com/en-us/library/system.typecode.aspx
            TypeCode[] UNCONVERTABLE_OBJECT_TYPES = new TypeCode[]
            {
                TypeCode.Boolean,
                TypeCode.Char,
                TypeCode.DateTime,
                TypeCode.DBNull,
                TypeCode.Decimal,
                TypeCode.Empty,
                TypeCode.Object,
                TypeCode.String
            };

            // Loop thought each object type and if an object's type code
            // match the anything from the above array, break out of the 
            // function as that type is not supported.
            System.Collections.IList list = Enum.GetValues(typeof(TypeCode));
            for (int i = 0; i < list.Count; i++)
            {
                TypeCode typeCode = (TypeCode)list[i];

                // Loop Each Object That We Are Unble To Use.
                // From There We Will Calculate The Result.
                // Return To The Caller.
                foreach (int unuseableType in UNCONVERTABLE_OBJECT_TYPES)
                {
                    if (Type.GetTypeCode(bytes.GetType()) == (TypeCode)unuseableType)
                    {
                        return string.Format("{0} is not a supported type", bytes.GetType());
                    }
                }
            }

            // Convert Object To long type and then to readable size.
            string[] suffix = new string[] { "bytes", "kb", "mb", "gb", "tb", "pb", "eb", "zb", "yb" };
            const long byteConversion = 1000;

            if (Convert.ToUInt64(bytes) == 0)
            {
                return "0.0 bytes";
            }

            int mag = (int)Math.Log(Convert.ToUInt64(bytes), byteConversion);
            double adjustedSize = (Convert.ToUInt64(bytes) / Math.Pow(1000, mag));
            return string.Format("{0:n2} {1}", adjustedSize, suffix[mag]);
        }

        /// <summary>
        /// Get the file mime type.
        /// </summary>
        /// <param name="filePathLocation">file path location</param>
        /// <returns></returns>
        public static string GetMimeType(this string filePathLocation)
        {
            try
            {
                if (filePathLocation.IsValidFilePath())
                {
                    string mimeType = "application/unknown";
                    string ext = Path.GetExtension(filePathLocation).ToLower();
                    Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

                    if (regKey != null && regKey.GetValue("Content Type") != null)
                    {
                        mimeType = regKey.GetValue("Content Type").ToString();
                    }
                    return mimeType;
                }
            }
            catch (Exception ex)
            {
                return "application/unknown";
            }

            return "application/unknown";
        }


        /// <summary>
        /// Calculate MD5
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static string TryGetFileCheckSum(this string filePath)
        {
            if (filePath.IsValidFilePath())
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (Stream stream = File.OpenRead(filePath))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            else
            {
                throw new Exception("Invalid file path");
            }
        }

        /// <summary>
        /// Replace Multiple Chars All At Once.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="charsToRemove"></param>
        /// <param name="replacementChar"></param>
        /// <returns></returns>
        public static string ReplaceMultiple(this string @string, char[] charsToRemove, char replacementChar)
        {
            return charsToRemove.Aggregate(@string, (str, charItem) => str.Replace(charItem, replacementChar));
        }

        /// <Summary>
        /// Write Coloured Text To The Console.
        /// </Summary>
        public static string PrintColouredText(this string @str, ConsoleColor foreground, EnumHelpers.Utils.OutputStyle outputStyle = EnumHelpers.Utils.OutputStyle.WRITE_LINE)
        {
            // Save the current console foreground colour.
            ConsoleColor currentForeground = Console.ForegroundColor;

            // Change the console foreground colour to the selected.
            Console.ForegroundColor = foreground;

            // Print the value with the selected output style.
            switch (outputStyle)
            {
                // Write Result
                case EnumHelpers.Utils.OutputStyle.WRITE:
                    Console.Write(@str);
                    break;
                // WriteLine Result
                case EnumHelpers.Utils.OutputStyle.WRITE_LINE:
                    Console.WriteLine(@str);
                    break;
            }

            // Restore the console foreground colour
            Console.ForegroundColor = currentForeground;

            // Return Nothing
            return null;
        }

        /// <Summary>
        /// Returns the file's creator
        /// </Summary>
        /// <param name="fileInfo">FileInfo Object</param>
        /// <returns></returns>
        public static string Owner(this FileInfo fileInfo)
        {            
            var zetaFileinfo = new ZlpFileInfo(fileInfo);
            return zetaFileinfo.Owner;
        }

        /// <Summary>
        /// Returns the file's creator
        /// </Summary>
        /// <param name="zetaFileInfo">ZetaFileInfo Object</param>
        /// <returns></returns>
        public static string Owner(this ZlpFileInfo zetaFileInfo) => zetaFileInfo.Owner;

        /// <summary>
        /// Extract A Hyper Link From an ahref property.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ExtractHyperLinkFromData(this string data)
        {
            return XElement.Parse(data)
                           .Descendants("a")
                           .Select(x => x.Attribute("href").Value)
                           .FirstOrDefault();
        }

        /// <summary>
        /// Remove Illegal Chars From Path
        /// [ File Name May Not Be Included ]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveIllegalCharsFromPath(this string str)
        {
            return Path.GetInvalidPathChars()
                       .Aggregate(str, (current, c) => current.Replace(c.ToString(), ""));
        }

        /// <summary>
        /// Extract Text From A Pdf.
        /// </summary>
        /// <param name="pdf_File"></param>
        /// <returns></returns>
        public static string TryExtractTextFromPDF(this string pdf_File)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                var pdfReader = new iTextSharp.text.pdf.PdfReader(pdf_File);
                StringBuilder text = new StringBuilder();
                var t = pdfReader.GetPageContent(pdfReader.NumberOfPages);
                return System.Text.Encoding.UTF8.GetString(t);
            }
        }

        /// <summary>
        /// Convert an Object To CXML
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToCXML(this object obj)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());                
                    xmlSerializer.Serialize(sw, obj);
                    return sw.ToString()
                             .Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", 
                                      "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!DOCTYPE cXML SYSTEM \"http://xml.cXML.org/schemas/cXML/1.2.021/cXML.dtd\">");
                }
            }
            catch (Exception ex)
            {
                // Object Couldn't Be Serialized.
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Convert an Object To XML.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXML(this object obj, bool utf8)
        {
            try
            {
                if (utf8)
                {
                    using (StringWriter sw = new Utf8StringWriter())
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                        xmlSerializer.Serialize(sw, obj);
                        return sw.ToString();
                    }
                }
                else
                {
                    using (StringWriter sw = new StringWriter())
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                        xmlSerializer.Serialize(sw, obj);
                        return sw.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Convert a string to a currency Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCurrency(this string value) => ToCurrency(Convert.ToDecimal(value));

        /// <summary>
        /// Convert an int to a currency Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCurrency(this int value) => ToCurrency(Convert.ToDecimal(value));

        /// <summary>
        /// Convert a double to a currency Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCurrency(this double value) => ToCurrency(Convert.ToDecimal(value));

        /// <summary>
        /// Convert a decimal to a currency Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCurrency(this decimal value) => string.Format("{0:C}",value);

        /// <summary>
        /// Get File Extension Type.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetExtension(this string filePath)
        {
            if (filePath.IsValidFilePath())
            {
                return System.IO.Path.GetExtension(filePath);
            }
            return null;
        }
 
        /// <summary>
        /// Reverse a String.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Reverse(this string str)
        {
            char[] chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Split a string in parts
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        public static IEnumerable<string> TrySplitInPartsOf(this string value, int numberOfParts)
        {
            for (int i = 0; i < value.Length; i += numberOfParts)
            {
                yield return value.Substring(i, Math.Min(numberOfParts, value.Length -i));
            }
        }
    
        /// <summary>
        /// Convert To Json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToJson(this string data)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(data).ToString() ?? null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Convert an Object To Json.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj).ToString() ?? null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Returns A String To Title Case
        /// Eg hello world --> Hello World
        /// </summary>
        /// <param name="str">Value</param>
        /// <returns></returns>
        public static string ToTitleCase(this string str)
        {
            if (str.Contains(' '))
            {
                StringBuilder sb = new StringBuilder();
                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    sb.Append(word.Substring(0,1).ToUpper() + word.Substring(1, word.Length -1) + " ");
                }
                sb.Length--;
                return sb.ToString();
            }
            return str.Substring(0,1).ToUpper() + str.Substring(1, str.Length - 1);
        }

        /// <Summary>
        /// Return the given string with camel casing.
        /// </Summary>
        /// <param name="str">Value</param>
        /// <returns>string [CamelCase]</returns>
        public static string ToCamelCase(this string str)
        {
            if (str.Contains(' '))
            {
                bool firstLoopComplete = false;
                StringBuilder sb = new StringBuilder();   
                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    if (!firstLoopComplete)
                    {
                        sb.Append(word + " ");
                        firstLoopComplete = true;
                    }
                    else
                    {
                        sb.Append(word[0].ToString().ToUpper() + word.Substring(1, word.Length -1) + " ");
                    }
                }
                sb.Length--;
                return sb.ToString();
            }
            return str;
        }

        /// <summary>
        /// Get File From Solution Folder Application.
        /// </summary>
        /// <param name="filePath">Path Within Solution</param>
        /// <returns></returns>
        public static string PathFromSolutionFolder(this string filePath)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filePath);
        }

        /// <summary>
        /// Get File From Solution Folder Web.
        /// </summary>
        /// <param name="filePath">Path Within Solution</param>
        /// <returns></returns>
        public static string PathFromSolutionFolderWeb(this string filePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,filePath);
        }

        /// <summary>
        /// Get The Last (n) of a String.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string GetLast(this string str, int n)
        {
            return n >= str.Length ? str.Substring(0, str.Length - n) : null;
        } 

        /// <summary>
        /// Return The Paths Directory Name Only.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDirectoryName(this string path)
        {
            return System.IO.Path.GetDirectoryName(path) ?? null;
        }

        /// <summary>
        /// Convert A Colour Into Hex.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToHex(this System.Drawing.Color color) => string.Format("#{0:X6}",color.ToArgb() & 0x00FFFFFF);
    
        /// <summary>
        /// Concatenate A String. End
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string ConcatenateString(this string str, int maxLength)
        {
            try
            {
                return str.Substring(0, maxLength -1);
            }
            catch
            {
                return str;
            }
        }
    
                /// <summary>
        /// Get Column Names
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetColumnNames(this System.Data.SqlClient.SqlConnection sqlConnection, string tableName)
        {
            if (sqlConnection.ConnectedToServerAsync())
            {
                using (System.Data.SqlClient.SqlCommand sqlCommand = new System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME =@tableName", sqlConnection))
                {
                    sqlCommand.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = tableName;

                    using (System.Data.SqlClient.SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            yield return Convert.ToString(sqlDataReader.GetValue(0));
                        }
                    }
                }

                sqlConnection.Close();
            }
        }
    }
}