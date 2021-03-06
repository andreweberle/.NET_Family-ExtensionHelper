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
        /// Perform A Swap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <typeparam name="T"></typeparam>
        public static void Swap<T>(ref T x, ref T y)
        {
            var temp = x;
            x = y;
            y = temp;
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
    }
}