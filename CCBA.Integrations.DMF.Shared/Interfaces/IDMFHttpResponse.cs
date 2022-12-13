using System.Net;
using System.Net.Http;

namespace CCBA.Integrations.DMF.Shared.Interfaces
{
    public interface IDMFHttpResponse
    {
        // string errorFileUrl { get; set; }

        string errorMessage { get; set; }
        bool errorsExist { get; set; }
        string executionId { get; set; }
        string exportedFileUrl { get; set; }
        HttpResponseMessage httpResponseMessage { get; set; }
        string logFileUrl { get; set; }
        string packageFileUrl { get; set; }
        string responseMessage { get; set; }
        HttpStatusCode Status { get; set; }
    }
}