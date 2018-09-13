using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace DevEnvLibrary
{
    [ServiceContract]
    public interface IDevEnvService
    {
        [OperationContract]
        [WebInvoke(Method = "*", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string AuthenticateUser(LoginUserRequest jsonValues);

        [OperationContract]
        [WebInvoke(Method = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        string BundleUpload(Stream stream);
        
    }

    [DataContract]
    public class LoginUserRequest
    {
        [DataMember]
        public string username { get; set; }

        [DataMember]
        public string password { get; set; }
    }
}
