using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EbbsSoft.ExtensionHelpers.StringHelpers;

namespace EbbsSoft.ExtensionHelpers.LongHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Calculate Directory Size.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        public static long DirectorySize(this DirectoryInfo dirInfo)
        {
            long size = 0;
            
            FileInfo[] fileInfo = dirInfo.GetFiles();
            foreach (var file in fileIndo)
            {
                size += file.Length;
            }

            DirectoryInfo[] subDirInfo = dirInfo.GetDirectories();
            
            foreach (var subDir in subDirInfo)
            {
                size += subDir.DirectorySize();
            }

            return size;
        }
    }
}