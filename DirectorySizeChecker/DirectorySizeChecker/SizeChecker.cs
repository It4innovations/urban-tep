using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectorySizeChecker.Data;

namespace DirectorySizeChecker
{
    public class SizeChecker
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static AppConfiguration config;

        public SizeChecker()
        {
            config = (AppConfiguration)System.Configuration.ConfigurationManager.GetSection("applicationConfiguration/results");
            Log.Debug(string.Format("MainDirectoryPath: {0}, DefaultMaximalSize: {1}MB, UserSettings: {2}",
                config.MainDirectoryPath,
                config.DefaultMaximalSizeMB,
                string.Join("; ", config.UserSettings.Select(x => x.UserName + " - " + x.MaximalSizeMB + "MB"))));
        }

        public void CheckDirectorySizes()
        {
            string[] subDirectories = Directory.GetDirectories(config.MainDirectoryPath);

            foreach (string subDirectoryPath in subDirectories)
            {
                UserSetting userSetting = config.UserSettings.FirstOrDefault(x => x.UserName == Path.GetFileName(subDirectoryPath));

                CheckIfWasExceededSize(
                    subDirectoryPath,
                    userSetting != null ? userSetting.MaximalSizeMB : config.DefaultMaximalSizeMB);
            }
        }

        private void CheckIfWasExceededSize(string subDirectoryPath, double maximalSizeMB)
        {
            double realSizeMB = GetDirectorySizeInMB(subDirectoryPath);

            if (realSizeMB > maximalSizeMB)
            {
                DeleteLastFilesExceedingSize(subDirectoryPath, realSizeMB, maximalSizeMB);
            }
        }

        private void DeleteLastFilesExceedingSize(string subDirectoryPath, double realSizeMB, double maximalSizeMB)
        {
            List<string> userDirectories = Directory.GetDirectories(subDirectoryPath).OrderBy(x => int.Parse(Path.GetFileName(x))).ToList();

            while (realSizeMB > maximalSizeMB && userDirectories.Count > 0)
            {
                string oldestDirectory = userDirectories.First();
                Log.Debug(string.Format("Deleting directory: {0} (size: {1:0.##}/{2}MB)", oldestDirectory, realSizeMB, maximalSizeMB));
                Directory.Delete(oldestDirectory, true);
                userDirectories.Remove(oldestDirectory);

                realSizeMB = GetDirectorySizeInMB(subDirectoryPath);
            }
        }

        private double GetDirectorySizeInMB(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            double bytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            return (bytes / 1024f) / 1024f;
        }
    }
}
