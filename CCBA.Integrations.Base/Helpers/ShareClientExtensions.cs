using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CCBA.Integrations.Base.Helpers
{
    public static class ShareClientExtensions
    {
        public static async Task<List<T>> ConsumeXMLFiles<T>(string shareName, string destFilePath,
            string connectionString)
        {
            var results = new List<T>();
            // Get a reference to a share and then create it
            var share = new ShareClient(connectionString, shareName);
            // Get a reference to a directory and create it
            var directory = share.GetRootDirectoryClient();

            var subdirectory = directory.GetSubdirectoryClient(destFilePath);
            await foreach (var item in subdirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    // Get a reference to a file and upload it
                    var file = directory.GetFileClient($"{destFilePath}/{item.Name}");
                    var fileStream = await file.OpenReadAsync(new ShareFileOpenReadOptions(false));
                    // Create an instance of the XmlSerializer specifying type.
                    var serializer = new XmlSerializer(typeof(T));

                    // Use the Deserialize method to restore the object's state.
                    var idef = (T)serializer.Deserialize(fileStream);
                    results.Add(idef);
                }
            }
            return results;
        }

        public static async Task<string> DownloadZip(string connectionString, string fileShareName, string directoryName, string fileName)
        {
            var cloudFileClient = new ShareClient(connectionString, fileShareName);

            // Get a reference to a directory and create it
            var directory = cloudFileClient.GetRootDirectoryClient();
            var subdir = directory.GetSubdirectoryClient(directoryName);

            // Get a reference to a file
            var file = subdir.GetFileClient(fileName);
            var dfile = await file.DownloadAsync();
            return dfile.Value.Details.CopySource.OriginalString;
        }

        public static async Task<Dictionary<string, Stream>> PackageFiles(string shareName, string destFilePath, string connectionString)
        {
            var results = new Dictionary<string, Stream>();
            // Get a reference to a share and then create it
            var share = new ShareClient(connectionString, shareName);
            // Get a reference to a directory and create it
            var directory = share.GetRootDirectoryClient();

            var subdirectory = directory.GetSubdirectoryClient(destFilePath);
            await foreach (var item in subdirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    // Get a reference to a file and upload it
                    var file = directory.GetFileClient($"{destFilePath}/{item.Name}");
                    var dfile = await file.DownloadAsync();

                    results.Add(item.Name, dfile.Value.Content);
                }
            }
            return results;
        }

        public static async Task UploadFileAsync(Stream stream, string shareName, string destFilePath, string connectionString)
        {
            // Get a reference to a share and then create it
            var share = new ShareClient(connectionString, shareName);
            // Get a reference to a directory and create it
            var directory = share.GetRootDirectoryClient();

            // Get a reference to a file and upload it
            var file = directory.GetFileClient(destFilePath);
            stream.Position = 0;
            await file.CreateAsync(stream.Length);
            await file.UploadRangeAsync(
                new HttpRange(0, stream.Length),
                stream);
        }

        public static async Task WriteXMLFiles<T>(List<T> objList, string share, string destinationPath, string fileext, string connectionString, string udiOrMdi)
        {
            foreach (var idef in objList)
            {
                var stream = new MemoryStream();
                var sw = new StreamWriter(stream, Encoding.ASCII);
                var xSerializer = new XmlSerializer(typeof(T));
                var nmsp = new XmlSerializerNamespaces();
                nmsp.Add("", "");

                var xWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true };

                var xmlWriter = XmlWriter.Create(sw, xWriterSettings);
                xSerializer.Serialize(xmlWriter, idef, nmsp);
                var filename = Guid.NewGuid().ToString();

                await UploadFileAsync(stream, share, $"{destinationPath}/{filename}.{fileext}{udiOrMdi}",
                    connectionString);
            }
        }
    }
}