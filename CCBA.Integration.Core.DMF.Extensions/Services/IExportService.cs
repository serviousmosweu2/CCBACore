using CCBA.Infinity;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CCBA.Integration.Core.DMF.Extensions.Services
{
    public interface IExportService
    {
        Task<IResult> ExportPackageAsync(ExportConfiguration exportSettings, string entryName, string logHeader);
    }

    public interface IResult
    {
        Stream ResultStream { get; }

        HttpStatusCode Status { get; }

        string ExportFile { get; }
    }
}