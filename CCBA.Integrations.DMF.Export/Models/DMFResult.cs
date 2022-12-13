using CCBA.Integrations.DMF.Export.Interfaces;
using CCBA.Integrations.DMF.Shared.Interfaces;
using System.Net;

namespace CCBA.Integrations.DMF.Export.Models
{
    internal class DMFResult : IDMFResult
    {
        private IDMFHttpResponse _dMFHttpResponse;

        public DMFResult(byte[] stream, HttpStatusCode status, string exportFile)
        {
            ResultStream = stream;
            Status = status;
            ExportFile = exportFile;
        }

        public string ErrorFileUrl { get; private set; }
        public string ErrorMessage { get; private set; }

        //public void Load(IDMFHttpResponse dMFHttpResponse)
        //{
        //    Status = dMFHttpResponse.Status;
        //    ExecutionId = dMFHttpResponse.executionId;
        //    PackageFileUrl = dMFHttpResponse.packageFileUrl;
        //    ResponseMessage = dMFHttpResponse.responseMessage;
        //    ErrorMessage = dMFHttpResponse.errorMessage;
        //    ErrorFileUrl = dMFHttpResponse.errorFileUrl;
        //}

        public string ExecutionId { get; private set; }
        public string ExportFile { get; }
        public string PackageFileUrl { get; private set; }
        public string ResponseMessage { get; private set; }
        public byte[] ResultStream { get; }

        public HttpStatusCode Status { get; private set; }
    }
}