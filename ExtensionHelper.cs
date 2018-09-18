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
using System.Xml;
using System.Xml.Linq;

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
        public static void MoveFilesTo(this string sourceDirectory, string destinationDirectory)
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
        public static void CreateDirectory(this string path) => Directory.CreateDirectory(path);

        /// <summary>
        /// Copy directory to a new destination
        /// </summary>
        /// <param name="sourceDirectory">Source Directory</param>
        /// <param name="targetDirectory">Targe Directory</param>
        public static void CopyDirectory(this string sourceDirectory, string targetDirectory)
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
                foreach (int unusableType in UNCONVERTABLE_OBJECT_TYPES)
                {
                    if (Type.GetTypeCode(bytes.GetType()) == (TypeCode)unusableType)
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
        public static string GetFileCheckSum(this string filePath)
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
        public static bool WriteToTextFile(this string textFilePath, string givenParameters)
        {
            if (textFilePath.IsValidFilePath())
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(textFilePath))
                    {
                        sw.WriteLine(givenParameters);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can not write File " + textFilePath + "  :" + ex.Message);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Invalid Path");
                return false;
            }
        }

        /// <summary>
        /// Create an Empty Text File
        /// </summary>
        /// <param name="textFilepath">Destination Location</param>
        /// <returns></returns>
        public static bool CreateTextFile(this string textFilepath)
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
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool CompareFileVersionTo(this string filePath0, string filePath)
        {
            // Create a file version object from the give path
            FileVersionInfo fV1 = FileVersionInfo.GetVersionInfo(filePath0);
            FileVersionInfo fV2 = FileVersionInfo.GetVersionInfo(filePath);

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
        public static string ReadValueFromXmlDoc(this XmlDocument xmlDocument, string elementTagName, string attribute)
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
        public static string ReadValueFromXmlDoc(this XmlDocument xmlDocument, string xPath)
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
                    sb.Append(word.ToUpperInvariant() + " ");
                }
                sb.Length--;
                return sb.ToString();
            }
            return str.ToUpperInvariant();
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
    }
}
