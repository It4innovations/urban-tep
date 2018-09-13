using HaaSAuthentificationLibrary.DAO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using UserProcessorDB;
using UserProcessorDB.Data;

namespace DevEnvLibrary
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DevEnvService : IDevEnvService
    {
        private static UserAuthentificationDAO haasUtil = new UserAuthentificationDAO(@"");

        public DevEnvService()
        {
        }

        public string AuthenticateUser(LoginUserRequest jsonValues)
        {
            CommonFunc.AddResponseHeaders();

            if (WebOperationContext.Current.IncomingRequest.Method == "OPTIONS")
            {
                return "Preflight check ok.";
            }
            else if (WebOperationContext.Current.IncomingRequest.Method == "POST")
            {
                try
                {
                    //check the user credentials
                    bool authenticated = false;
                    if (haasUtil.IsUserInDB(jsonValues.username))
                    {
                        string userPasswd = haasUtil.GetUserPasswordByUserName(jsonValues.username);
                        if (userPasswd == jsonValues.password)
                            authenticated = true;
                    }

                    if (authenticated)
                    {
                        //prepare user folders
                        CommonFunc.PrepareFolders(jsonValues.username);

                        //create session cookie
                        CommonFunc.CreateToken(jsonValues.username);

                        return "Authentication succesful.";
                    }
                    else return "Authentication failed.";
                }
                catch (Exception ex)
                {
                    return "Error:" + ex.Message;
                }
            }
            else return "Authentication failed.";
        }

        public string BundleUpload(Stream stream)
        {
            CommonFunc.AddResponseHeaders();

            if (WebOperationContext.Current.IncomingRequest.Method == "OPTIONS")
            {
                return "Preflight check ok.";
            }
            else if (WebOperationContext.Current.IncomingRequest.Method == "POST")
            {
                try
                {
                    string ret = "";

                    //try to parse input file stream
                    MultipartParser parser = new MultipartParser();
                    parser.ParseSimple(stream, Encoding.UTF8);
                    if (parser.Success)
                    {
                        //save package
                        string fullPath = CommonFunc.packageDir + parser.Username + @"\Packages\" + parser.Filename;
                        using (FileStream writer = new FileStream(fullPath, FileMode.Create))
                        {
                            writer.Write(parser.FileContents, 0, parser.FileContents.Length);
                        }

                        //deploy package
                        bool created = CommonFunc.DeployPackage(parser.Username, parser.Filename);

                        if (created)
                        {
                            //create support ticket
                            //CommonFunc.SubmitTicket(parser.Username, parser.Filename);

                            ret = "Upload complete. New support ticket with the deployment request was automatically submitted.";
                        }
                        else ret = "Processor with this name is already created. Processor scripts were updated automatically.";

                        return ret;
                    }
                    return "Upload failed.";
                }
                catch (Exception ex)
                {
                    return "Error:" + ex.Message;
                }
            }
            else return "Not authorized.";
        }
    }
}
