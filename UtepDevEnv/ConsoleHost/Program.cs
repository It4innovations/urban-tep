using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost svcHost = null;
            try
            {
                svcHost = new ServiceHost(typeof(DevEnvLibrary.DevEnvService));
                svcHost.Open();

                Console.WriteLine("Service is Running at following address:");
                Console.WriteLine("\nSERVICEURL");
                Console.WriteLine("\nType 'quit' to exit.");
                while (Console.ReadLine() != "quit") { continue; }
                Console.WriteLine("\nExiting ...");
            }
            catch (Exception eX)
            {
                svcHost.Close();
                svcHost = null;
                Console.WriteLine("\nService can not be started \nError Message: " + eX.Message + "\nStackTrace:\n" + eX.StackTrace);
                Console.WriteLine("\nInner: " + eX.InnerException);
                Console.WriteLine("\nSource: " + eX.Source);
                Console.WriteLine("\nTargetSite: " + eX.TargetSite);
                Console.ReadKey();
            }
        }
    }
}
