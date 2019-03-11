using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EbbsSoft.ExtensionHelpers.VoidHelpers;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;
using EbbsSoft.ExtensionHelpers.StringHelpers;

namespace EbbsSoft.ExtensionHelpers.VoidHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Print Event Style Message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="eventType"></param>
        /// <param name="colour"></param>
        public static async void PrintEvent(this string context, EnumHelpers.Utils.PrintEventType eventType, System.ConsoleColor colour)
        {
            Task task = Task.Run(() =>
            {
                $"\n[{eventType.ToString()}]".PrintColouredText(colour, EnumHelpers.Utils.OutputStyle.WRITE);
                Console.Write("\n" + context);
                System.Threading.Thread.Sleep(500);
            });
            await task;
        }

        /// <summary>
        /// Print Event Style Message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="eventType"></param>
        /// <param name="colour"></param>
        public static async void PrintEvent(this string context, string eventType, System.ConsoleColor colour)
        {
            Task task = Task.Run(() =>
            {              
                $"\n[{eventType.ToString()}]".PrintColouredText(colour, EnumHelpers.Utils.OutputStyle.WRITE);
                Console.Write("\n" + context);
                System.Threading.Thread.Sleep(500);
            });
            await task;
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

        /// <Summary>
        /// Write Coloured Text To The Console.
        /// </Summary>
        public static void PrintColouredText(this ConsoleColor colour, ConsoleColor foreground, string value, EnumHelpers.Utils.OutputStyle outputStyle = EnumHelpers.Utils.OutputStyle.WRITE_LINE)
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
                    Console.Write(value);
                break;
                // WriteLine Result
                case EnumHelpers.Utils.OutputStyle.WRITE_LINE:
                    Console.WriteLine(value);
                break;
            }

            // Restore the console foreground colour
            Console.ForegroundColor = currentForeground;
        }
    
                /// <summary>
        /// Moves files from one directory to another.
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        public static void TryMoveFilesTo(this string sourceDirectory, string destinationDirectory)
        {
            if (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsValidDirectoryPath(sourceDirectory) && (EbbsSoft.ExtensionHelpers.BooleanHelpers.Utils.IsValidDirectoryPath(destinationDirectory)))
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
    
    }
}