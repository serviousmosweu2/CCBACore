using System.Net;

namespace CCBA.Integrations.DMF.Export.Interfaces
{
    public interface IDMFResult
    {
        string ErrorFileUrl { get; }
        string ExecutionId { get; }
        string ExportFile { get; }
        string PackageFileUrl { get; }
        string ResponseMessage { get; }
        byte[] ResultStream { get; }
        HttpStatusCode Status { get; }
        //void Load(IDMFHttpResponse dMfResponse);
    }
}