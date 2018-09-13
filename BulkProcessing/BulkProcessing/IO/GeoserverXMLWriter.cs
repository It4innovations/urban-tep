using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace BulkProcessing.IO
{
    public class GeoserverXMLWriter
    {
        public string CreateRequestBody(string subDirectoryPath)
        {
            StringBuilder requestBodyText = new StringBuilder();
            requestBodyText.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestBodyText.AppendLine("<wps:Execute xmlns:ows=\"http://www.opengis.net/ows/1.1\" service=\"WPS\" version=\"1.0.0\" xmlns:wps=\"http://www.opengis.net/wps/1.0.0\">");
            requestBodyText.AppendLine("<ows:Identifier>gs:TestService</ows:Identifier>");
            requestBodyText.AppendLine("<wps:DataInputs>");
            requestBodyText.AppendLine("<wps:Input>");
            requestBodyText.AppendLine("<ows:Identifier>inputFolderPath</ows:Identifier>");
            requestBodyText.AppendFormat("<wps:Data><wps:LiteralData>{0}</wps:LiteralData></wps:Data>", subDirectoryPath);
            requestBodyText.AppendLine("</wps:Input>");
            requestBodyText.AppendLine("</wps:DataInputs>");
            requestBodyText.AppendLine("<wps:ResponseForm>");
            requestBodyText.AppendLine("<wps:ResponseDocument storeExecuteResponse=\"true\" status=\"true\">");
            requestBodyText.AppendLine("<wps:Output>");
            requestBodyText.AppendLine("<ows:Identifier>Result</ows:Identifier>");
            requestBodyText.AppendLine("</wps:Output>");
            requestBodyText.AppendLine("</wps:ResponseDocument>");
            requestBodyText.AppendLine("</wps:ResponseForm>");
            requestBodyText.AppendLine("</wps:Execute>");

            return requestBodyText.ToString();
        }

        public string ParseGeoserverResponse(HttpWebResponse response)
        {
            string responseUrl = null;

            using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
            {
                responseUrl = responseStream.ReadToEnd();
                //using (XmlReader responseXmlReader = XmlReader.Create(responseStream, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore }))
                //{
                //    while (responseXmlReader.Read())
                //    {
                //        if (responseXmlReader.IsStartElement("wps:ExecuteResponse"))
                //        {
                //            responseUrl = responseXmlReader.GetAttribute("statusLocation");
                //            break;
                //        }
                //    }
                //}
            }

            return responseUrl;
        }
    }
}
