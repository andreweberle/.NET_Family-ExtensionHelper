using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EbbsSoft.ExtensionHelpers.StringHelpers;

namespace EbbsSoft.ExtensionHelpers.BooleanHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Check if an object is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNull<T>(this T obj) => obj == null ? true : false;

        /// <summary>
        /// Check if an object is not null.
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNotNull<T>(this T obj) => obj != null ? false: true;

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
            while (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsFileLocked(filePath));
            
            return false;
        }

        
        /// <summary>
        /// Wait For A File.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool WaitForFile(this string filePath, double milliseconds)
        {
            do
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(milliseconds));
            }
            while (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsFileLocked(filePath));
            
            return false;
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
        /// Check if a T type list is empty.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsEmpty<T>(this List<T> list) => list.Count > 0 ? false : true;

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
        /// Check If Given Number Is An Odd Number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this int value) => value % 2 != 0;

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
                var mailAddress = new System.Net.Mail.MailAddress(emailAddress);
                return mailAddress.Address == emailAddress;
            }
            catch
            {
                return false;
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
        /// Returns if the given file path exists.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static bool IsValidFilePath(this string fullFilePath) => File.Exists(fullFilePath);

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
        /// Check If The Given Address Is A PostOffice Or Not.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsPostOfficeBox(this string address)
        {
            return new Regex(@"(?i)\b(?:p(?:ost)?\.?\s*(?:[o0](?:ffice)?)?\.?\s*b(?:[o0]x)?|b[o0]x)").IsMatch(address);
        }
                        /// <summary>
        /// Moves files from one directory to another.
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        public static bool TryMoveFilesTo(this string sourceDirectory, string destinationDirectory)
        {
            if (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsValidDirectoryPath(sourceDirectory) && (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsValidDirectoryPath(destinationDirectory)))
            {
                // Loop through each file and move it to the destination directory
                Directory.GetFiles(sourceDirectory).ToList()
                                                   .ForEach(file => File.Move(file, destinationDirectory + "\\" + Path.GetFileName(file)));
                
                // TODO: Fix
                return true;
            }
            else
            {
                throw new Exception("Source Or Destination Not Valid");
            }
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="path">path to new directory</param>
        public static bool TryCreateDirectory(this string path)
        {
            Directory.CreateDirectory(path);
            return IsValidDirectoryPath(path);
        }

        /// <summary>
        /// Copy File To Destination
        /// </summary>
        /// <param name="path"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static bool TryCopyFile(this string path, string destination)
        {
            File.Copy(path, destination);
            return destination.IsValidFilePath();
        }

        /// <summary>
        /// Copy directory to a new destination
        /// </summary>
        /// <param name="sourceDirectory">Source Directory</param>
        /// <param name="targetDirectory">Targe Directory</param>
        public static bool TryCopyDirectory(this string sourceDirectory, string targetDirectory)
        {
            if (sourceDirectory.IsValidDirectoryPath())
            {
                _CopyDirectory(sourceDirectory, targetDirectory);
                return IsValidDirectoryPath(targetDirectory);
            }
            else
            {
                throw new Exception("Source Directory Not Valid");
            }
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
        /// Set A String Char Length
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static bool StringMaxLength(this string str, int maxLength)
        {
            if (str.IsEmpty())
            {
                return true;
            }
            else
            {
                return str.Length <= maxLength ? true : false;
            }
        }

        /// <summary>
        /// Check If The Given Decimal Is OK.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool IsValidDecimal(this string n)
        {
            return Decimal.TryParse(n, out decimal result);
        }

        /// <summary>
        /// Check If The Given Decimal Is OK.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsValidDecimal<T>(this T obj) => IsValidDecimal(Convert.ToString(obj));
    }
}