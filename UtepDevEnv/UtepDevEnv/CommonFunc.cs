using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using System.IO.Compression;
using System.Net.Mail;
using System.Xml;
using System.Diagnostics;

namespace UtepDevEnv
{
    public class UserToken
    {
        public String token { get; set; }
        public DateTime date { get; set; }
    }

    public class CommonFunc
    {
        //session tokens for users
        private static Dictionary<string, UserToken> userSession = new Dictionary<string, UserToken>();
        
        //package directory
        public static string packageDir = @"PDIR";

        //allowed origins
        private static List<string> allowedOrigins = new List<string>(){
            "ORIGIN",
            "ORIGIN"
        };

        /// <summary>
        /// Add response headers.
        /// </summary>
        public static void AddResponseHeaders()
        {
            string origin = WebOperationContext.Current.IncomingRequest.Headers["Origin"];
            if (allowedOrigins.Contains(origin))
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", origin);

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "*");
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
        }


        /// <summary>
        /// Create secured session token.
        /// </summary>
        /// <param name="userId">User id.</param>
        public static void CreateToken(string username)
        {
            //create session token
            string sessionToken = Guid.NewGuid().ToString();
            //WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie] = string.Format("sessionToken={0}; Path=/; secure", sessionToken);
            WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie] = string.Format("sessionToken={0}", sessionToken);
            
            UserToken ut = new UserToken();
            ut.token = sessionToken;
            ut.date = DateTime.Now;

            //store sessionToken
            if (userSession.ContainsKey(username))
                userSession[username] = ut;
            else userSession.Add(username, ut);
        }

        /// <summary>
        /// Compare stored sessionToken with the request sessionToken from the user request.
        /// </summary>
        /// <param name="userId">User id.</param>
        public static void CheckSessionToken(string username)
        {
            bool valid = false;

            string sessionToken = "";
            if (WebOperationContext.Current.IncomingRequest.Headers.AllKeys.Contains("Cookie"))
            {
                string cookieHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Cookie].ToString();
                string[] cookies = cookieHeader.Split(';');
                foreach (string cookie in cookies)
                {
                    string[] nameValue = cookie.Trim().Split('=');
                    if (nameValue[0] == "sessionToken")
                    {
                        sessionToken = nameValue[1];
                        break;
                    }
                }

                if (userSession.ContainsKey(username))
                {
                    UserToken ut = userSession[username];
                    if ((DateTime.Now - ut.date).Minutes > 60)
                    {
                        userSession.Remove(username);
                    }
                    else
                    {
                        if (ut.token == sessionToken)
                            valid = true;
                    }
                }
            }

            if (valid == false)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Forbidden;
                throw new Exception("Access forbidden");
            }
        }

        /// <summary>
        /// Create user filesystem structure.
        /// </summary>
        /// <param name="username">Username.</param>
        public static void PrepareFolders(string username)
        {
            if (!Directory.Exists(packageDir + username + @"\"))
                Directory.CreateDirectory(packageDir + username + @"\");
            if (!Directory.Exists(packageDir + username + @"\Packages\"))
                Directory.CreateDirectory(packageDir + username + @"\Packages\");
            if (!Directory.Exists(packageDir + username + @"\Extracted\"))
                Directory.CreateDirectory(packageDir + username + @"\Extracted\");
        }

        /// <summary>
        /// Deploy user procesor
        /// </summary>
        public static void DeployPackage(string username, string packagename)
        {
            //unziping
            string pathToZip = packageDir + username + @"\Packages\" + packagename;
            UnzipFolder(pathToZip);

            //build docker image and push to repository
            DockerBuildAndPush(username, packageDir + username + @"\Extracted\" + packagename.Replace(".zip", ""));
        }

        /// <summary>
        /// Build docker image and push to repository.
        /// </summary>
        /// <param name="processorPath"></param>
        private static void DockerBuildAndPush(string username, string processorPath)
        {
            if (File.Exists(processorPath + @"\descriptor.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(processorPath + @"\descriptor.xml");

                XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("utep", "SCHEME");

                string procName = doc.DocumentElement.SelectSingleNode("/utep:descriptor/utep:processor/utep:packaging/utep:name", manager).InnerText;
                string procVersion = doc.DocumentElement.SelectSingleNode("/utep:descriptor/utep:processor/utep:packaging/utep:version", manager).InnerText;
                
                string longProcName = "urbantep-" + procName + "-" + procVersion;

                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + @"dockerBuild.bat" + " " + username + " " + procName + " " + longProcName);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                var process = Process.Start(processInfo);

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("output>>" + e.Data);
                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("error>>" + e.Data);
                process.BeginErrorReadLine();

                process.WaitForExit();

                Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
            }
        }

        /// <summary>
        /// Unzip folder.
        /// </summary>
        /// <param name="zipPath">Zip path.</param>
        private static void UnzipFolder(string zipPath)
        {
            string pathToExtract = zipPath.Replace(".zip", "").Replace("Packages", "Extracted") + @"\";
            if (Directory.Exists(pathToExtract))
                Directory.Delete(pathToExtract, true);
            Directory.CreateDirectory(pathToExtract);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(pathToExtract, entry.FullName));
                    if (entry.FullName.Contains(".zip"))
                        UnzipFolder(pathToExtract + entry.Name);
                }
            }
            if (zipPath.Contains("Extracted"))
                File.Delete(zipPath);
        }

        public static void SubmitTicket(string username, string packagename)
        {
            MailMessage mailmsg = new MailMessage();
            mailmsg.From = new MailAddress("MAIL");
            mailmsg.To.Add(new MailAddress("MAIL"));
            mailmsg.Subject = "DevEnv WPS deployment request";
            mailmsg.Body = "username: " + username + " \r\npackagename: " + packagename;

            //string pathToXml = packageDir + username + @"\Extracted\" + packagename.Replace(".zip", "") + @"\descriptor.xml";
            //mailmsg.Attachments.Add(new Attachment(pathToXml));

            SmtpClient client = new SmtpClient();
            //client.Port = 25;
            client.Host = "SMTP";
            //client.Timeout = 10000;
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.UseDefaultCredentials = false;
            //client.Credentials = new System.Net.NetworkCredential("user", "Password");
            
            client.Send(mailmsg);
        }
    }
}