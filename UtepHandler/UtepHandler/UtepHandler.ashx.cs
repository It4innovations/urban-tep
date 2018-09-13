using HaaSAuthentificationLibrary.DAO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace UtepHandler
{
    /// <summary>
    /// Summary description for RequestHandler
    /// </summary>
    public class UtepHandler : IHttpHandler
    {
        private static UserAuthentificationDAO haasUtil = new UserAuthentificationDAO(@"");

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string handlerName = "HANDLER";
        private static readonly string geoserverName = "GEOSERVER";
        private static readonly string proxyName = "PROXY";

        private static readonly string schemaLocation = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/wps/1.0.0 http://schemas.opengis.net/wps/1.0.0/wpsExecute_response.xsd\" ";
        private static readonly string serviceInstance = "serviceInstance=\"SERVICE/ows?\" ";

        string requestContent = "";
        string responseContent = "";

        public void ProcessRequest(HttpContext context)
        {
            //original request
            log.Info("Original request URL: " + context.Request.HttpMethod + " " + context.Request.Url.AbsoluteUri);
            string headers = "";
            foreach (var headerKey in context.Request.Headers.AllKeys)
            {
                if (headers != "")
                    headers += "; ";
                headers += headerKey + ": " + context.Request.Headers[headerKey];
            }
            log.Info("Original request HEADERS: " + headers);
            //read request content
            if (context.Request.ContentType == "text/xml" || context.Request.ContentType == "application/json" || context.Request.ContentType == "application/xml" || context.Request.ContentType == "text/plain")
            {
                System.IO.Stream body = context.Request.InputStream;
                Encoding encoding = Encoding.UTF8;
                if(context.Request.ContentEncoding != null)
                    encoding = context.Request.ContentEncoding;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding))
                {
                    requestContent = reader.ReadToEnd();
                    log.Info("Original request CONTENT: " + requestContent);
                }
            }

            //parse HEADER header
            //if(true)
            if (context.Request.Headers.AllKeys.Contains("HEADER"))
            {
                try
                {
                    //header
                    string remoteUser = "";
                    if (context.Request.Headers.AllKeys.Contains("HEADER"))
                        remoteUser = context.Request.Headers["HEADER"];
                    else log.Error("Header HEADER is missing." + context.Request.Url.AbsoluteUri);

                    bool authenticated = false;

                    //check the user credentials
                    string userPasswd = "";
                    if (haasUtil.IsUserInDB(remoteUser))
                    {
                        userPasswd = haasUtil.GetUserPasswordByUserName(remoteUser);
                        authenticated = true;
                    }
                    else
                    {
                        userPasswd = haasUtil.CreateUser(remoteUser);
                        authenticated = true;
                    }
           
                    if (authenticated && context.Request.Url.AbsoluteUri.Contains(handlerName))
                    {
                        //extract HEADER header
                        string remoteRef = "null";
                        if (context.Request.Headers.AllKeys.Contains("HEADER"))
                            remoteRef = context.Request.Headers["HEADER"];
                        remoteRef = remoteRef.Replace("-", "_");

                        //forward request to the geoserver
                        string originalUrl = context.Request.Url.AbsoluteUri.Replace(handlerName, geoserverName);
                        //log.Info("Proxy request: " + context.Request.Url.AbsoluteUri);
                        log.Info("User '" + remoteUser + "' authorized, forwarding request: " + originalUrl);
                        //new request
                        var externalRequest = (HttpWebRequest)WebRequest.Create(originalUrl);
                        CopyRequest(context.Request, externalRequest, requestContent, remoteUser, userPasswd, remoteRef);

                        //decompress gzip response body
                        try
                        {
                            externalRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        }
                        catch (Exception e)
                        {}

                        var externalResponse = (HttpWebResponse)externalRequest.GetResponse();
                        //read response content
                        if (externalResponse.ContentType == "text/xml" || externalResponse.ContentType == "application/json" || externalResponse.ContentType == "application/xml" || externalResponse.ContentType == "text/plain")
                        {
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(externalResponse.GetResponseStream(), Encoding.ASCII))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }
                        //forward response back to the proxy
                        CopyResponse(context.Response, externalResponse, responseContent);
                    }
                    else
                    {
                        //user is not authorized
                        SendResponse(context, HttpStatusCode.Unauthorized, "User is not authorized.");
                        log.Error("User '" + remoteUser + "' is not authorized - " + context.Request.Url.AbsoluteUri);
                    }
                    
                }
                catch (Exception e)
                {
                    //exception
                    SendResponse(context, HttpStatusCode.InternalServerError, "UtepHandler error: " + e.Message);
                    log.Error("UtepHandler error: " + e.Message + " StackTrace:" + e.StackTrace);
                }
            }
            else
            {
                //user is not authorized
                SendResponse(context, HttpStatusCode.Unauthorized, "User is not authorized.");
                log.Error("Header HEADER is missing - " + context.Request.Url.AbsoluteUri);
            }
        }

        public static bool ParseJson(string json)
        {
            //todo parse json

            return true;
        }

        public static void SendResponse(HttpContext context, HttpStatusCode code, string responseMsg)
        {
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "text/plain";
            context.Response.CacheControl = "no-cache";
            context.Response.Write(responseMsg);
        }

        public static void CopyRequest(HttpRequest originalRequest, HttpWebRequest externalRequest, string requestContent, string remoteUser, string userPasswd, string remoteRef)
        {
            externalRequest.Method = originalRequest.HttpMethod;

            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in originalRequest.Headers.AllKeys)
            {
                if (!WebHeaderCollection.IsRestricted(headerKey)) 
                    externalRequest.Headers[headerKey] = originalRequest.Headers[headerKey]; 
            }

            // Copy restricted headers
            if (originalRequest.AcceptTypes != null)
            {
                if (originalRequest.AcceptTypes.Any())
                    externalRequest.Accept = string.Join(",", originalRequest.AcceptTypes);
            }
            if (originalRequest.ContentType != null)
                externalRequest.ContentType = originalRequest.ContentType;
            if (originalRequest.UrlReferrer != null)
                externalRequest.Referer = originalRequest.UrlReferrer.AbsoluteUri;
            if (originalRequest.UserAgent != null)
                externalRequest.UserAgent = originalRequest.UserAgent;

            // Copy content (if content body is allowed)
            if (originalRequest.HttpMethod != "GET" && originalRequest.HttpMethod != "HEAD" && originalRequest.ContentLength > 0)
            {
                if (requestContent == "")
                {
                    var destinationStream = externalRequest.GetRequestStream();
                    originalRequest.InputStream.Position = 0;
                    originalRequest.InputStream.CopyTo(destinationStream);
                    destinationStream.Close();
                }
                else
                {
                    //add to request options parameter with the users credetial for the job submition
                    if (requestContent.Contains("wps:Execute "))
                    {
                        if (requestContent.Contains("<ows:Identifier>options</ows:Identifier>"))
                        {
                            //remove newlines from the content
                            requestContent = requestContent.Replace("\r\n", "");
                            //add content with credentials
                            string match = "<wps:Input><ows:Identifier>options</ows:Identifier><wps:Data><wps:LiteralData>";
                            int ind = requestContent.IndexOf(match);
                            if (ind != -1)
                            {
                                ind += match.Length;
                                requestContent = requestContent.Insert(ind, "userName:" + remoteUser + ",userPasswd:" + userPasswd + ",remoteRef:" + remoteRef + ",");
                            }
                        } else {
                            //remove newlines from the content
                            requestContent = requestContent.Replace("\r\n", "");
                            //add credentials to options
                            string match = "</wps:DataInputs>";
                            int ind = requestContent.IndexOf(match);
                            if (ind != -1)
                            {
                                requestContent = requestContent.Insert(ind, "<wps:Input><ows:Identifier>options</ows:Identifier><wps:Data><wps:LiteralData>userName:"
                                    + remoteUser + ",userPasswd:" + userPasswd + ",remoteRef:" + remoteRef + "</wps:LiteralData></wps:Data></wps:Input>");
                            }
                        }

                        //format input parameters for user processors
                        if (requestContent.Contains("<ows:Identifier>default:"))
                        {
                            requestContent = requestContent.Replace("\r\n", "");

                            //productionName
                            requestContent = ReplaceParamInRequestContent("productionName", requestContent, "-", "_");
                            requestContent = ReplaceParamInRequestContent("productionName", requestContent, " ", "_");
                            //minDate
                            requestContent = ReplaceParamInRequestContent("minDate", requestContent, "-", "_");
                            //maxDate
                            requestContent = ReplaceParamInRequestContent("maxDate", requestContent, "-", "_");
                            //regionWkt
                            requestContent = ReplaceParamInRequestContent("regionWkt", requestContent, " ", "_");
                            requestContent = ReplaceParamInRequestContent("regionWkt", requestContent, "-", "X");
                            //regionBB
                            requestContent = ReplaceParamInRequestContent("regionBB", requestContent, " ", "");
                            requestContent = ReplaceParamInRequestContent("regionBB", requestContent, "-", "X");

                            //remove QUOTATION output, only metadata_result should be present
                            requestContent = FormatOutputsInRequestContent(requestContent);
                        }
                    }

                    log.Info("Forwarded request CONTENT: " + requestContent);

                    //encode and write request content
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] bytes = encoding.GetBytes(requestContent);
                    Stream destinationStream = externalRequest.GetRequestStream();
                    destinationStream.Write(bytes, 0, bytes.Length);
                    destinationStream.Close();
                }
            }
        }

        public static void CopyResponse(HttpResponse originalResponse, HttpWebResponse externalResponse, string responseContent)
        {
            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in externalResponse.Headers.AllKeys)
            {
                if (!WebHeaderCollection.IsRestricted(headerKey))
                    originalResponse.Headers[headerKey] = externalResponse.Headers[headerKey]; 
            }

            // Copy restricted headers
            if (externalResponse.ContentType != null)
                originalResponse.ContentType = externalResponse.ContentType;

            // Copy content
            if (responseContent == "")
            {
                externalResponse.GetResponseStream().CopyTo(originalResponse.OutputStream);
            }
            else
            {
                //replace proxy name
                responseContent = responseContent.Replace(geoserverName, proxyName);
                //add aditional schemas
                if (responseContent.Contains("wps:ExecuteResponse") && !responseContent.Contains("xsi:schemaLocation"))
                {
                    int ind = responseContent.IndexOf("xml:lang=\"en\"");
                    responseContent = responseContent.Insert(ind, schemaLocation);
                }
                if (responseContent.Contains("wps:ExecuteResponse") && !responseContent.Contains("serviceInstance"))
                {
                    int ind = responseContent.IndexOf("xml:lang=\"en\"");
                    responseContent = responseContent.Insert(ind, serviceInstance);
                }

                //replace outputs for user processors (Describe process)
                if (responseContent.Contains("<wps:ProcessDescriptions ") && responseContent.Contains("<ows:Identifier>default:"))
                {
                    responseContent = responseContent.Replace("\r\n", "");
                    string original = "<ProcessOutputs><Output><ows:Identifier>result_metadata</ows:Identifier><ows:Title>result_metadata</ows:Title><LiteralOutput/></Output></ProcessOutputs>";
                    string replacement = "<ProcessOutputs><Output><ows:Identifier>result_metadata</ows:Identifier><ows:Title>result_metadata</ows:Title><ComplexOutput><Default><Format><MimeType>"
                        + "application/xml</MimeType></Format></Default><Supported><Format><MimeType>application/xml</MimeType></Format></Supported></ComplexOutput></Output><Output><ows:Identifier>"
                        + "QUOTATION</ows:Identifier><ows:Title>QUOTATION</ows:Title><ComplexOutput><Default><Format><MimeType>application/json</MimeType></Format></Default><Supported><Format>"
                        + "<MimeType>application/json</MimeType></Format></Supported></ComplexOutput></Output></ProcessOutputs>";
                    responseContent = responseContent.Replace(original, replacement);
                }

                //change output for result_metadata/QUOTATION
                if (responseContent.Contains("<wps:ExecuteResponse ") && responseContent.Contains("<ows:Identifier>default:"))
                {
                    if (responseContent.Contains("mimeType=\"application/atom+xml\""))
                    {
                        //log.Info("RESPONSE TYPE: result_metadata");
                        
                        //extract response content
                        responseContent = responseContent.Replace("\r\n", "");
                        int startI = responseContent.IndexOf("<wps:Data><wps:LiteralData>");
                        int endI = responseContent.IndexOf("</wps:LiteralData></wps:Data>");

                        string content = responseContent.Substring(startI + "<wps:Data><wps:LiteralData>".Length, endI - startI - "<wps:Data><wps:LiteralData>".Length);
                        content = content.Replace("&lt;", "<");
                        content = content.Replace("&gt;", ">");
                        
                        //replace output formating
                        string original = responseContent.Substring(startI, endI + "</wps:LiteralData></wps:Data>".Length - startI);
                        string replacement = "<wps:Data><wps:ComplexData mimeType=\"application/xml\">" + content + "</wps:ComplexData></wps:Data>";
                        responseContent = responseContent.Replace(original, replacement);
                    }
                    else if (responseContent.Contains("{\"id\":\"quotationOffer\""))
                    {
                        //log.Info("RESPONSE TYPE: QUOTATION");

                        //extract response content
                        responseContent = responseContent.Replace("\r\n", "");
                        int startI = responseContent.IndexOf("<wps:Data><wps:LiteralData>");
                        int endI = responseContent.IndexOf("</wps:LiteralData></wps:Data>");
                        string content = responseContent.Substring(startI + "<wps:Data><wps:LiteralData>".Length, endI - startI - "<wps:Data><wps:LiteralData>".Length);

                        //replace output formating
                        int startR = responseContent.IndexOf("<ows:Identifier>result_metadata");
                        int endR = responseContent.IndexOf("</wps:LiteralData>") + "</wps:LiteralData>".Length;
                        string original = responseContent.Substring(startR, endR - startR);
                        string replacement = "<ows:Identifier>QUOTATION</ows:Identifier><ows:Title>Quotation offering</ows:Title><wps:Data><wps:ComplexData mimeType=\"application/json\">" + content + "</wps:ComplexData>";
                        responseContent = responseContent.Replace(original, replacement);
                    }
                }

                log.Info("Forwarded response CONTENT: " + responseContent);
                
                //write to response stream
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(responseContent);
                Stream destinationStream = originalResponse.OutputStream;
                destinationStream.Write(bytes, 0, bytes.Length);
                destinationStream.Close();
            }
        }

        public static string FormatOutputsInRequestContent(string requestContent)
        {
            log.Info("INSIDE");
            string retContent = requestContent.Replace("\r\n", "");

            //remove the quotation output
            string qOutput = "";
            if (retContent.Contains("<wps:Output mimeType=\"application/json\"><ows:Identifier>QUOTATION</ows:Identifier>"))
                qOutput = "<wps:Output mimeType=\"application/json\"><ows:Identifier>QUOTATION</ows:Identifier>";
            if (retContent.Contains("<wps:Output><ows:Identifier>QUOTATION</ows:Identifier>"))
                qOutput = "<wps:Output><ows:Identifier>QUOTATION</ows:Identifier>";

            log.Info("qOutput: " + qOutput);
            
            if (qOutput != "")
            {
                int startI = retContent.IndexOf(qOutput);
                string tempSub = retContent.Substring(startI, retContent.Length - startI);
                log.Info("tempSub: " + tempSub);
                int endI = startI + tempSub.IndexOf("</wps:Output>") + "</wps:Output>".Length;
                string original = retContent.Substring(startI, endI - startI);
                log.Info("original: " + original);
                retContent = retContent.Replace(original, "");
            }

            //add metadata_result if not included
            if (!retContent.Contains("<ows:Identifier>result_metadata</ows:Identifier></wps:Output>"))
            {
                int tempIndex = retContent.IndexOf("</wps:ResponseDocument>");
                retContent = retContent.Insert(tempIndex, "<wps:Output mimeType=\"application/xml\"><ows:Identifier>result_metadata</ows:Identifier></wps:Output>");
            }

            return retContent;
        }

        public static string ReplaceParamInRequestContent(string param, string requestContent, string replaceWhat, string replaceWith)
        {
            int startI = requestContent.IndexOf("<ows:Identifier>" + param + "</ows:Identifier><wps:Data><wps:LiteralData>");
            log.Info(startI);
            if (startI != -1)
            {
                startI = startI + ("<ows:Identifier>" + param + "</ows:Identifier><wps:Data><wps:LiteralData>").Length;

                string substring = requestContent.Substring(startI, requestContent.Length - startI);
                int endI = startI + substring.IndexOf("</wps:LiteralData></wps:Data>");
                string content = requestContent.Substring(startI, endI - startI);
                content = content.Replace(replaceWhat, replaceWith);

                //replace
                int tempI1 = requestContent.IndexOf("<ows:Identifier>" + param + "</ows:Identifier><wps:Data><wps:LiteralData>");
                string tempSub = requestContent.Substring(tempI1, requestContent.Length - tempI1);
                int tempI2 = tempI1 + tempSub.IndexOf("</wps:LiteralData></wps:Data>") + "</wps:LiteralData></wps:Data>".Length;
                string original = requestContent.Substring(tempI1, tempI2 - tempI1);
                log.Info("ORIGINAL: " + original);
                string replacement = "<ows:Identifier>" + param + "</ows:Identifier><wps:Data><wps:LiteralData>" + content + "</wps:LiteralData></wps:Data>";
                log.Info("REPLACEMENT: " + replacement);
                requestContent = requestContent.Replace(original, replacement);
            }
            return requestContent;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}