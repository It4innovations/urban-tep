using System;
using System.Collections.Generic;

namespace BulkProcessing
{
    class Program
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Debug("Bulk processing launched...");

            try
            {
                BulkProcess bp = new BulkProcess();
                Dictionary<string, DateTime> newDirectories = bp.GetNewDirectories();
                List<string> responseUrls = bp.ProcessNewFiles(newDirectories);
            }
            catch (Exception e)
            {
                Log.Error("Error in BulkProcessing: ", e);
            }
        }
    }
}
