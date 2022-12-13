using CCBA.Infinity;
using System.Threading.Tasks;

namespace CCBA.Integration.Core.DMF.Extensions.Services
{
    public interface IImportService
    {
        Task<DMFHttpResponse> QueueImport(ImportConfiguration importConfiguration, byte[] streamBytes, string entityName);
    }
}