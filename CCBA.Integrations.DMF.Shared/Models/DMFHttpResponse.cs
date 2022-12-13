using CCBA.Integrations.DMF.Shared.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace CCBA.Integrations.DMF.Shared.Models
{
    public class DMFHttpResponse : List<DMFHttpResponse>, IDMFHttpResponse
    {
        public Dictionary<string, string> ErrorFileUrls { get; set; } = new Dictionary<string, string>();
        public string errorMessage { get; set; }
        public bool errorsExist { get; set; }
        public string executionId { get; set; }
        public string exportedFileUrl { get; set; }
        public HttpResponseMessage httpResponseMessage { get; set; }
        public string logFileUrl { get; set; }
        public string packageFileUrl { get; set; }
        public DMFHttpResponse Pi { get; set; }
        public string responseMessage { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}