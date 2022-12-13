using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CCBA.Integration.Core.DMF.Extensions
{
    public interface IDMFFileManager
    {
        Task<List<T>> ConsumeXMLFiles<T>(string shareName, string destFilePath, string connectionString);
        Task<string> DownloadZip(string connectionString, string fileShareName, string directoryName, string fileName);
        Task UploadFileAsync(Stream stream, string shareName, string destFilePath, string connectionString);
        Task WriteXMLFiles<T>(List<T> objList, string share, string destinationPath, string fileext, string connectionString, string udiOrMdi);
    }
}