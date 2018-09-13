using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using BulkProcessing.DB;
using BulkProcessing.IO;

namespace BulkProcessing
{
    public class BulkProcess
    {
        private static BulkProcessDAO dao;
        private static AppConfiguration config;

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BulkProcess()
        {
            dao = new BulkProcessDAO();
            dao.InitializeDB();
            config = (AppConfiguration)System.Configuration.ConfigurationManager.GetSection("applicationConfiguration/bulkProcess");
            Log.Debug(string.Format("MainDirectoryPath: {0}, ModifiedDate: {1}min",
                config.MainDirectoryPath,
                config.ModifiedDateMinutes));
        }

        public Dictionary<string, DateTime> GetNewDirectories()
        {
            DirectoryChecker checker = new DirectoryChecker(config.MainDirectoryPath);
            Dictionary<string, DateTime> newDictionary = new Dictionary<string, DateTime>();

            Dictionary<string, DateTime> diskDictionary = checker.GetDirectoryInfo();
            Dictionary<string, DateTime> dbDictionary = dao.GetDirectoryInfo();

            foreach (KeyValuePair<string, DateTime> diskInfo in diskDictionary)
            {
                if ((DateTime.Now - diskInfo.Value).TotalMinutes > config.ModifiedDateMinutes)
                {
                    KeyValuePair<string, DateTime> currentInfo = dbDictionary.FirstOrDefault(x => x.Key == diskInfo.Key);

                    if (currentInfo.Key == null)
                    {
                        newDictionary.Add(diskInfo.Key, diskInfo.Value);
                    }
                }
            }

            Log.Debug(string.Format("Directories on disk: {0}, directories in DB: {1}, directories for processing: {2}",
                diskDictionary.Count(),
                dbDictionary.Count(),
                newDictionary.Count()));

            return newDictionary;
        }

        public List<string> ProcessNewFiles(Dictionary<string, DateTime> newDirectories)
        {
            List<string> responseUrls = new List<string>();

            foreach (KeyValuePair<string, DateTime> newDirectory in newDirectories)
            {
                Log.Debug(string.Format("Sending directory path on WPS... (name: {0}, modified date: {1:dd.MM.yyyy HH:mm:ss})",
                            newDirectory.Key,
                            newDirectory.Value));

                responseUrls.Add(SendPostOnWPS(Path.Combine(config.MainDirectoryPath, newDirectory.Key)));
                
                Log.Debug("Response obtained.");
            }
            dao.InsertNewDirectories(newDirectories, responseUrls);

            return responseUrls;
        }

        public string SendPostOnWPS(string subDirectoryPath)
        {
            GeoserverXMLWriter geoserverXMLWriter = new GeoserverXMLWriter();
            HttpWebRequest geoserverRequest = HttpWebRequest.CreateHttp(config.GeoServerWPSUrl);
            geoserverRequest.Method = "POST";
            geoserverRequest.Headers.Add("USER", "USER");
            string requestBodyText = geoserverXMLWriter.CreateRequestBody(subDirectoryPath);
            using (StreamWriter requestBody = new StreamWriter(geoserverRequest.GetRequestStream()))
            {
                requestBody.Write(requestBodyText.ToString());
            }
            HttpWebResponse response = (HttpWebResponse)geoserverRequest.GetResponse();

            //return response.ToString();
            return geoserverXMLWriter.ParseGeoserverResponse(response);
        }
    }
}
