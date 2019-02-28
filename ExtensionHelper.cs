using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ZetaLongPaths;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace EbbsSoft
{
    /// <summary>
    /// An Extension Helper For The .Net Family.
    /// </summary>
    public static class ExtensionHelper
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

        /// <summary>
        /// Returns if the given path is vaild or not.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidDirectoryPath(this string path) => Directory.Exists(path);

        /// <summary>
        /// Check if an object array has a duplicate object,
        /// </summary>
        /// <param name="arrayList"></param>
        /// <returns></returns>
        public static bool? HasDuplicateItem<T>(this T[] arrayList)
        {
            // Check if the list is an array
            // This should always been an array
            // but just in case we will check anyways
            if (arrayList.GetType().IsArray)
            {
                // Create a T type List that will hold our unique items
                List<T> uniqueItemsArray = new List<T>();

                // Loop through each item and if there is a duplicate item
                // print it to the console.
                // Once we have looped through each of the items
                // we will return true or false for if any of the lists
                // are in both the list and the unique List.
                arrayList.ToList().ForEach(item =>
                {
                    if (!uniqueItemsArray.Contains(item))
                    {
                        uniqueItemsArray.Add(item);
                    }
                    else
                    {
                        Console.WriteLine("{0} is a duplicate item", item);
                    }
                });
                return uniqueItemsArray.Intersect(arrayList).Any();
            }
            // Return nothing because 
            // the list was not an array.
            return null;
        }

        /// <summary>
        /// Returns if the given file path exists.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static bool IsValidFilePath(this string fullFilePath) => File.Exists(fullFilePath);

        /// <summary>
        /// Moves files from one directory to another.
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        public static void TryMoveFilesTo(this string sourceDirectory, string destinationDirectory)
        {
            if (IsValidDirectoryPath(sourceDirectory) && (IsValidDirectoryPath(destinationDirectory)))
            {
                // Loop through each file and move it to the destination directory
                Directory.GetFiles(sourceDirectory).ToList()
                                                   .ForEach(file => File.Move(file, destinationDirectory + "\\" + Path.GetFileName(file)));
            }
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="path">path to new directory</param>
        public static void TryCreateDirectory(this string path) => Directory.CreateDirectory(path);

        /// <summary>
        /// Copy directory to a new destination
        /// </summary>
        /// <param name="sourceDirectory">Source Directory</param>
        /// <param name="targetDirectory">Targe Directory</param>
        public static void TryCopyDirectory(this string sourceDirectory, string targetDirectory)
        {
            if (sourceDirectory.IsValidDirectoryPath())
            {
                _CopyDirectory(sourceDirectory, targetDirectory);
                return;
            }
            throw new Exception("Source Directory Not Valid");
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
            // Check If The Operating System If Supported.
            if (IsWindows)
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
            else
            {
                throw new Exception(string.Format("--OPERATING SYSTEM ({0}) NOT SUPPORTED--", RuntimeInformation.OSDescription));
            }
            
            return null;
        }

        /// <Summary>
        /// Check If The Operating System Is Windows
        /// </Summary>
        public static bool IsWindows
        {
            get
            {
                // Return true is the os is windows, else return false.
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? true : false;
            }
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
                Console.WriteLine("Invalid file path");
                throw new Exception("Invalid file path");
            }
        }

        /// <summary>
        /// Write text to a text file
        /// </summary>
        /// <param name="textFilePath">Destination Of File</param>
        /// <param name="givenParameters"></param>
        public static bool TryWriteToTextFile(this string textFilePath, string data, bool AppendText)
        {
            if (!textFilePath.IsValidFilePath())
            {
                textFilePath.TryCreateTextFile();
            }
            
            try
            {
                using (StreamWriter sw = new StreamWriter(textFilePath,AppendText))
                {
                    sw.WriteLine(data);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not write File " + textFilePath + "  :" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create an Empty Text File
        /// </summary>
        /// <param name="textFilepath">Destination Location</param>
        /// <returns></returns>
        public static bool TryCreateTextFile(this string textFilepath)
        {
            try
            {
                // Creates a blank text file.
                using (FileStream fs = File.Create(textFilepath))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("File not created ! " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Compare File Versions
        /// </summary>
        /// <param name="filePath0"></param>
        /// <param name="filePath1"></param>
        /// <returns></returns>
        public static bool CompareFileVersionTo(this string filePath0, string filePath1)
        {
            // Create a file version object from the give path
            FileVersionInfo fV1 = FileVersionInfo.GetVersionInfo(filePath0);
            FileVersionInfo fV2 = FileVersionInfo.GetVersionInfo(filePath1);

            // Create a Version Object from the FileVersionInfo Object
            Version v1 = new Version(fV1.FileMajorPart, fV1.FileMinorPart, fV1.FileBuildPart);
            Version v2 = new Version(fV2.FileMajorPart, fV2.FileMinorPart, fV2.FileBuildPart);

            // If the base file (V1) is lower than V2
            // than this means that the base file is out of date.
            return v1 < v2;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        /// Validate an email address.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(this string emailAddress)
        {
            // Create a MailAddress object and pass in the emailAdress string
            // If it works than using the MailAddress validation has accepted the
            // email address, if it doesn't than the MailAddress object has not
            // accepted the email address and we will return false from the catch statement.
            try
            {
                System.Net.Mail.MailAddress mailAddress = new System.Net.Mail.MailAddress(emailAddress);
                return mailAddress.Address == emailAddress;
            }
            catch
            {
                return false;
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

        /// <summary>
        /// Return the number of bytes
        /// for a string object.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="encodingFormat"></param>
        /// <returns></returns>
        public static int? GetBytesCount(this string @string, EncodingFormat encodingFormat = EncodingFormat.ASCII)
        {
            switch (encodingFormat)
            {
                case EncodingFormat.ASCII:
                    return System.Text.Encoding.ASCII.GetByteCount(@string);

                case EncodingFormat.UTF7:
                    return System.Text.Encoding.UTF7.GetByteCount(@string);

                case EncodingFormat.UTF8:
                    return System.Text.Encoding.UTF8.GetByteCount(@string);

                case EncodingFormat.UT32:
                    return System.Text.Encoding.UTF32.GetByteCount(@string);

                case EncodingFormat.UNICODE:
                    return System.Text.Encoding.Unicode.GetByteCount(@string);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Return true or false
        /// if a string object meets the
        /// minimum length requirement.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="minimumLength"></param>
        /// <returns></returns>
        public static bool IsValidLength(this string @string, int minimumLength) => (@string.Length <= minimumLength) ? true : false;

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

        /// <summary>
        /// Convert an XML Document to an XDocument
        /// </summary>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        /// <summary>
        /// Convert an XDocument to an XML Document
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (XmlNodeReader nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

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

        /// <Summary>
        /// Write Coloured Text To The Console.
        /// </Summary>
        public static void PrintColouredText(this ConsoleColor colour, ConsoleColor foreground, string value, OutputStyle outputStyle = OutputStyle.WRITE_LINE)
        {
            // Save the current console foreground colour.
            ConsoleColor currentForeground = Console.ForegroundColor;

            // Change the console foreground colour to the selected.
            Console.ForegroundColor = foreground;

            // Print the value with the selected output style.
            switch (outputStyle)
            {
                // Write Result
                case OutputStyle.WRITE:
                    Console.Write(value);
                break;
                // WriteLine Result
                case OutputStyle.WRITE_LINE:
                    Console.WriteLine(value);
                break;
            }

            // Restore the console foreground colour
            Console.ForegroundColor = currentForeground;
        }

        /// <Summary>
        /// Write Coloured Text To The Console.
        /// </Summary>
        public static string PrintColouredText(this string @str, ConsoleColor foreground, OutputStyle outputStyle = OutputStyle.WRITE_LINE)
        {
            // Save the current console foreground colour.
            ConsoleColor currentForeground = Console.ForegroundColor;

            // Change the console foreground colour to the selected.
            Console.ForegroundColor = foreground;

            // Print the value with the selected output style.
            switch (outputStyle)
            {
                // Write Result
                case OutputStyle.WRITE:
                    Console.Write(@str);
                    break;
                // WriteLine Result
                case OutputStyle.WRITE_LINE:
                    Console.WriteLine(@str);
                    break;
            }

            // Restore the console foreground colour
            Console.ForegroundColor = currentForeground;

            // Return Nothing
            return null;
        }

        /// <summary>
        /// Bubble Sort
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static dynamic BubbleSort(this int[] array)
        {
            try
            {
                // We will use the bubble sort algorthim
                for (int i = 0; i != array.Length; i++)
                {
                    for (int j = i + 1; j != array.Length; j++)
                    {
                        // if the current element is higher
                        // then the next element, we will swap
                        // their positions around.
                        if (array[i] > array[j])
                        {
                            Swap(ref array[i], ref array[j]);
                        }
                    }
                }

                // Once the algorithm is completed,
                // we will return the array back to
                // the caller.
                return array;
            }
            catch (Exception ex)
            {
                // If an exception is thrown,
                // the dynamic object given is most likely
                // unable to be sorted.
                throw new Exception(ex.StackTrace);
            } 
        }

        /// <summary>
        /// Preform a swap between two strings
        /// objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Swap(ref string x, ref string y)
        { 
            var temp = x;
            x = y;
            y = temp;
        }

        /// <summary>
        /// Preform a swap between two integers
        /// objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Swap(ref int x, ref int y)
        {
            // Swap Objects Without An 
            // Addictional Variable. 
            x = x + y;
            y = x - y;
            x = x - y;
        }

        /// <summary>
        /// Preform a swap between two objects
        /// objects.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Swap(ref object x, ref object y)
        {
            var temp = x;
            x = y;
            y = temp;
        }

        /// <summary>
        /// C# Has no built in Folder Copy Function like VB.NET
        /// https://msdn.microsoft.com/en-us/library/system.io.directoryinfo.aspx
        /// Call Method using eg CopyDirectory(sourceDir,destinationDir);
        /// </summary>
        /// <param name="sourceDirectory">Source Directory Path</param>
        /// <param name="targetDirectory">Destination Directory Path</param>
        private static void _CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);
            CopyAll(diSource, diTarget);
        }
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Create a new directory with the target directories name
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
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

        /// <summary>
        /// Validate An Australian Phone Number
        /// eg 0354xxxxxx | 0412xxxxxx
        /// </summary>
        /// <param name="phoneNumber">Australian Phone Number</param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            if (phoneNumber.Length == 10 && phoneNumber.StartsWith("0"))
            {
                foreach (char c in phoneNumber)
                {
                    if (!char.IsDigit(c))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
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
        /// System Path Watcher.
        /// </summary>
        /// <param name="path">path to watch</param>
        /// <param name="filter">watch path for files with extension x</param>
        /// <returns></returns>     
        public static object WatchPath(this string path, string filter = "*.*")
        {
            // Create a file watcher object.
            var fileWatcher = new FileSystemWatcher(path)
            {
                Filter = filter,
                EnableRaisingEvents = true
            };

            string msg = null;

            fileWatcher.Created += (sender,e) =>
            {
                msg = string.Format("{0} has been created at {1}", e.Name, Path.GetDirectoryName(e.FullPath));
                Console.WriteLine(msg);
            };

            fileWatcher.Deleted += (sender, e) =>
            {
                msg = string.Format("{0} has been deleted at {1}", e.Name, Path.GetDirectoryName(e.FullPath));
                Console.WriteLine(msg);
            };

            fileWatcher.Changed += (sender, e) =>
            {
                msg = string.Format("{0} has been changed at {1}", e.Name, Path.GetDirectoryName(e.FullPath));
                Console.WriteLine(msg);
            };

            fileWatcher.Renamed += (sender, e) =>
            {
                msg = string.Format("{0} has been renamed at {1}", e.Name, Path.GetDirectoryName(e.FullPath));
                Console.WriteLine(msg);
            };

            fileWatcher.Error += (sender, e) =>
            {
                msg = string.Format("Error : {0}", e.GetException());
                Console.WriteLine(msg);
            };

            fileWatcher.Disposed += (sender, e) =>
            {
                msg = string.Format("No Longer Watching : {0}", path);
                Console.WriteLine(msg);
            };

            return null;      
        }

        /// <Summary>
        /// Check If The Given File Path
        /// Is Locked/Not Useable
        /// </Summary>
        public static bool IsFileLocked(this string filePath)
        {
            // Check if the file path is valid.
            if (filePath.IsValidFilePath())
            {
                try
                {
                    using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, FileAccess.Read))
                    {
                        return false;
                    }
                }
                catch
                {
                    // if the above function fails,
                    // we will return true as that
                    // means the file is indeed locked.
                    return true;
                }
            }
            Console.WriteLine("The given file path does not exist".PrintColouredText(ConsoleColor.Red));
            return true;
        }

        /// <summary>
        /// Check For Empty String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check String For Only Digits
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigitsOnly(this string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validate Luhun (Mod10)
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static bool ValidateLuhn(this string sum)
        {
            // check whether input string is null or empty
            if (!sum.IsDigitsOnly() || !sum.IsEmpty())
            {
                return false;
            }

            int digits = sum.Where((e) => e >= '0' && e <= '9')
                            .Reverse()
                            .Select((e, i) => (e - 48) * (i % 2 == 0 ? 1 : 2))
                            .Sum((e) => e / 10 + e % 10);
                            
            return digits % 10 == 0;
        }

        /// <summary>
        /// Validate Luhn (Mod10)
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static bool ValidateLuhn(this int sum)
        {
            return ValidateLuhn(Convert.ToString(sum));
        }

        /// <summary>
        /// Convert Stream To Byte Array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToByteArray(this Stream stream)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer,0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Check If Given Number Is An Odd Number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this int value) => value % 2 != 0;

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
        /// Is File Read Only?
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool IsReadOnly(this FileInfo fileInfo)
        {
            return fileInfo.IsReadOnly;
        }

        /// <summary>
        /// Is File Read Only
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsReadOnly (this string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.IsReadOnly;
        }

        /// <summary>
        /// Change File To Read Only
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool AddReadOnlyAttribute(this FileInfo fileInfo)
        {
            fileInfo.IsReadOnly = true;
            return IsReadOnly(fileInfo);
        }

        /// <summary>
        /// Change File To Read Only
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool AddReadOnlyAttribute(this string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.IsReadOnly = true;
            return IsReadOnly(fileInfo);
        }
        
        /// <summary>
        /// Remove Read Only Property
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool RemoveReadOnlyAttribute(this FileInfo fileInfo)
        {
            fileInfo.IsReadOnly = false;
            return IsReadOnly(fileInfo);
        }

        /// <summary>
        /// Remove Read Only Property
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool RemoveReadOnlyAttribute(this string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.IsReadOnly = false;
            return IsReadOnly(fileInfo);
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
        /// Byte Array To Memory Stream
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static Stream ToMemoryStream(this byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }
    
        /// <summary>
        /// Convert an Object To XML
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
        /// Check if a T type list is empty.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsEmpty<T>(this List<T> list) => list.Count > 0 ? false : true;

        /// <summary>
        /// return a string to a memory stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(this string value, EncodingFormat encodingType)
        {
            switch (encodingType)
            {
                case EncodingFormat.UTF7:
                    return new MemoryStream(Encoding.UTF7.GetBytes(value ?? ""));

                case EncodingFormat.UTF8:
                    return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));

                case EncodingFormat.UT32:
                    return new MemoryStream(Encoding.UTF32.GetBytes(value ?? ""));

                case EncodingFormat.UNICODE:
                    return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));

                case EncodingFormat.ASCII:
                    return new MemoryStream(Encoding.ASCII.GetBytes(value ?? ""));
            }
            return null;
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
        /// Reset a Tables Seed.
        /// </summary>
        /// <param name="sqlConn">Connection To SQL Database</param>
        /// <param name="tableName">TableName</param>
        /// <param name="seed">Seed Starting Point</param>
        /// <returns></returns>
        public static bool ResetTableSeed(this SqlConnection sqlConn, string tableName, int seedIndex)
        {
            string RESET_SEED = $"DBCC CHECKIDENT ('[{tableName}]', RESEED, @value)";
            using (SqlConnection sqlConnection = new SqlConnection(sqlConn.ConnectionString))
            {   
                using (SqlCommand sqlCommand = new SqlCommand(RESET_SEED, sqlConn))
                {
                    sqlCommand.Parameters.AddWithValue("@value",seedIndex.ToString());
                    
                    if (sqlConnection.ConnectedToServerAsync())
                    {
                        return sqlCommand.ExecuteNonQuery() > 1 ? true : false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Check Connection To SQL Server Async
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static bool ConnectedToServerAsync(this SqlConnection sqlConnection)
        {
            bool isConnected = false;
            Task task = Task.Run(async() =>
            {
                try
                {
                    await sqlConnection.OpenAsync();
                    isConnected = true;
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            });

            Task.WaitAll(task);
            return isConnected;
        }

        /// <summary>
        /// Add & Count Only Business Days.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="addDays"></param>
        /// <returns></returns>
        public static DateTime AddWorkingDays(this DateTime date, int addDays)
        {
            int temp = addDays < 0 ? -1 : 1;
            DateTime newDate = date;
            while (addDays != 0)
            {
                newDate = newDate.AddDays(temp)
                ;
                if (newDate.DayOfWeek != DayOfWeek.Saturday 
                                      && newDate.DayOfWeek 
                                      != DayOfWeek.Sunday 
                                      && !newDate.IsHoliday())
                {
                    addDays -= temp;
                }
            }
            return newDate;
        }

        /// <summary>
        /// Check If Its a Public Holiday
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsHoliday(this DateTime date)
        {
            // Victoria Australia, Public Holidays.
            DateTime[] holidays = new DateTime[] 
            { 
                new DateTime(DateTime.Now.Year,01,01), // New Year's Day.
                new DateTime(DateTime.Now.Year,01,28), // Australia Day Holiday.
                new DateTime(DateTime.Now.Year,03,11), // Labour Day.
                new DateTime(DateTime.Now.Year,04,19), // Good Friday.
                new DateTime(DateTime.Now.Year,04,20), // Day following Good Friday.
                new DateTime(DateTime.Now.Year,04,21), // Easter Sunday.
                new DateTime(DateTime.Now.Year,04,22), // Easter Monday.
                new DateTime(DateTime.Now.Year,04,25), // Anzac Day
                new DateTime(DateTime.Now.Year,06,10), // Queen's Birthday
                new DateTime(DateTime.Now.Year,09,27), // AFL Grand Final Friday
                new DateTime(DateTime.Now.Year,10,05), // Melbourne Cup Day *
                new DateTime(DateTime.Now.Year,12,25), // Christmas Day
                new DateTime(DateTime.Now.Year,12,26), // Boxing Day
            };
            return holidays.Contains(date.Date);
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
        /// Unix TimeStamp To DateTime
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime UnixToDateTime(this double unixTimestamp)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            long unixTimeStampInTicks = (long) (unixTimestamp * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Local);
        }

        /// <summary>
        /// DateTime To Unix TimeStamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static Double DateTimeToUnix(this DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double) unixTimeStampInTicks / TimeSpan.TicksPerSecond;
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
        /// String to Stream.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Stream ToStream(this string data)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(data);
            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Wait For A File.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool WaitForFile(this string filePath)
        {
            do
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5000));
            }
            while (EbbsSoft.ExtensionHelper.IsFileLocked(filePath));
            
            return false;
        }

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
        /// Reverse a Number Set.
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static int Reverse(this int[] numbers)
        {
            StringBuilder sb = new StringBuilder();
            Array.ForEach(numbers, x => sb.Append(x));
            return Convert.ToInt32(Reverse(sb.ToString()));
        }

        /// <summary>
        /// Convert From Celsius To Fahrenheit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ToFahrenheit(this double c) => ((9.0 / 5.0) * c) + 32;

        /// <summary>
        /// Convert From Celsius To Fahrenheit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ToFahrenheit(this int c) => ((9.0 / 5.0) * Convert.ToDouble(c)) + 32;

        /// <summary>
        /// Convert From Fahrenheit To Celsius
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double ToCelsius(this double f) => (5.0 / 9.0) * (f - 32);

        /// <summary>
        /// Convert From Fahrenheit To Celsius
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double ToCelsius(this int f) => (5.0 / 9.0) * (Convert.ToDouble(f) - 32);

        /// <summary>
        /// Get DateTimeFromInternet
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="username">username required, Sign up for an account http://api.geonames.org</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromInternet(this DateTime dt, string username)
        {
            // Raw Data Place Holder.
            dynamic rawData = null;

            // Tuple For Corrdinates.
            (decimal lat, decimal lng) = GetCoordinates();

            Task task = Task.Run(async() =>
            {
                string url = $"http://api.geonames.org/timezoneJSON?lat={lat}&lng={lng}&username={username}";
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage httpResponseMessage= await httpClient.GetAsync(url))
                using (HttpContent httpContent = httpResponseMessage.Content)
                {
                    // Attempt To Read Data.
                    rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await httpContent.ReadAsStringAsync());
                }
            });
            Task.WaitAny(task);

            // If Raw Data Is Null, Return MinValue For DateTime.
            return rawData == null ? DateTime.MinValue : (DateTime)Convert.ToDateTime(Convert.ToString(rawData["time"]));
        }

        /// <summary>
        /// Get Current Coordinates
        /// </summary>
        /// <returns></returns>
        public static (decimal lat, decimal lng) GetCoordinates()
        {
            dynamic rawData = null;

            Task task = Task.Run(async() =>
            {
                string url = "http://ip-api.com/json/";
                using (HttpClient httpClient= new HttpClient())
                using (HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url))
                using (HttpContent httpContent = httpResponseMessage.Content)
                {
                    rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await httpContent.ReadAsStringAsync());
                }
            });

            Task.WaitAll(task);
            return (Convert.ToDecimal(rawData["lat"]), Convert.ToDecimal(rawData["lon"]));
        }
    }
}
