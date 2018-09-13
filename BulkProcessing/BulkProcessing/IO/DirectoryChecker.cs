using System;
using System.Collections.Generic;
using System.IO;

namespace BulkProcessing.IO
{
    public class DirectoryChecker
    {
        private static string MainDirectoryPath;

        public DirectoryChecker(string path)
        {
            MainDirectoryPath = path;
        }

        public Dictionary<string, DateTime> GetDirectoryInfo()
        {
            Dictionary<string, DateTime> infoDictionary = new Dictionary<string, DateTime>();
            string[] subDirectories = Directory.GetDirectories(MainDirectoryPath);

            foreach (string subDirectoryPath in subDirectories)
            {
                infoDictionary.Add(
                    Path.GetFileName(subDirectoryPath),
                    Directory.GetLastWriteTime(subDirectoryPath));
            }

            return infoDictionary;
        }
    }
}
