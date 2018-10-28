using System;
using System.Collections.Generic;
using System.Text;
using EbbsSoft;

namespace MyApplication
{
    class Program
    {
        static void Main()
        {           
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "foo.txt";
            string fullPath = System.IO.Path.Combine(desktopPath,fileName);

            Console.WriteLine(fullPath.IsFileLocked());
        }
    }
}
