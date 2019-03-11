using System;
using System.IO;

namespace EbbsSoft.ExtensionHelpers.ObjectHelpers
{
    public static class Utils
    {
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
    }
}