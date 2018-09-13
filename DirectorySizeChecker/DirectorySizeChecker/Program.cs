using System;

namespace DirectorySizeChecker
{
    class Program
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Debug("Checking directories sizes...");

            try
            {
                new SizeChecker().CheckDirectorySizes();
            }
            catch (Exception e)
            {
                Log.Error("Error in CheckDirectorySizes: ", e);
            }
        }
    }
}
