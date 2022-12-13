using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integration.Core.DMF.Extensions
{
    public interface IDMFPackageManager
    {
        byte[] CreateExcel<T>(List<T> idefFiles);
        Task<byte[]> CreatePackageAsync<T>(List<T> objFiles, string connectionString, string shareName, string folderName);
    }
}