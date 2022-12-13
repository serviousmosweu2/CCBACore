using System.Net.Http;

namespace CCBA.Infinity
{
    public class DMFHttpResponse
    {
        public HttpResponseMessage httpResponseMessage { get; set; }
        public string errorFileUrl { get; set; }
        public string exportedFileUrl { get; set; }
        public string responseMessage { get; set; }
        public string exportErrorMessage { get; set; }
        public string logFileUrl { get; set; }
        public string Status { get; set; }
    }
}