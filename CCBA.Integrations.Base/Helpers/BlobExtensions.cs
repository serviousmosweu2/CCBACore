using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CCBA.Integrations.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    public static class BlobExtensions
    {
        public static string CreatePackage(this List<DmfXMLFiles> dmfXmlFiles, string source)
        {
            var tempPath = Path.GetTempPath();
            var target = $@"{tempPath}\{Guid.NewGuid()}.zip";
            File.Copy(source, target);

            foreach (var xmlFile in dmfXmlFiles)
            {
                var path = $@"{tempPath}\{xmlFile.FileName}";
                File.WriteAllText(path, xmlFile.XmlContent, Encoding.ASCII);

                using var zipToOpen = new FileStream(target, FileMode.Open);
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

                var filename = $"{xmlFile.FileName}";
                archive.CreateEntryFromFile(path, filename);
            }

            return target;
        }

        public static async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var blobClient = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), containerName, blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public static async Task<Uri> SaveToBlobContainer(this byte[] data, string path, string fileName)
        {
            var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), path);
            await blobContainerClient.CreateIfNotExistsAsync();
            await using (var ms = new MemoryStream(data))
            {
                await blobContainerClient.DeleteBlobIfExistsAsync(fileName);
                await blobContainerClient.UploadBlobAsync(fileName, ms);
            }
            return blobContainerClient.Uri;
        }

        public static async Task<Uri> SaveToBlobContainer(this byte[] data, string path, string fileName, string connectionString)
        {
            var blobContainerClient = new BlobContainerClient(connectionString, path);
            await blobContainerClient.CreateIfNotExistsAsync();
            await using (var ms = new MemoryStream(data))
            {
                await blobContainerClient.DeleteBlobIfExistsAsync(fileName);
                await blobContainerClient.UploadBlobAsync(fileName, ms);
            }
            return blobContainerClient.Uri;
        }

        public static async Task<Uri> SaveToBlobContainer(this Stream data, string path, string fileName)
        {
            var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), path);
            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.DeleteBlobIfExistsAsync(fileName);
            await blobContainerClient.UploadBlobAsync(fileName, data);
            return blobContainerClient.Uri;
        }

        public static async Task<Uri> UploadBlobAsync(string containerName, string fileName, BinaryData content, bool overwrite = false)
        {
            var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(content, overwrite);

            return blobClient.Uri;
        }

        public static string UploadBlobWithSasUri(string containerName, string fileName, BinaryData content, TimeSpan expires = default, bool throwOnError = false)
        {
            return UploadBlobWithSasUriAsync(containerName, fileName, content, expires, throwOnError).GetAwaiter().GetResult();
        }

        public static async Task<string> UploadBlobWithSasUriAsync(string containerName, string fileName, BinaryData content, TimeSpan expires = default, bool throwOnError = false)
        {
            try
            {
                var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), containerName);
                if (!await blobContainerClient.ExistsAsync())
                {
                    await blobContainerClient.CreateAsync();
                }

                var blobClient = blobContainerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(content, true);

                if (expires == default) expires = TimeSpan.FromDays(25 * 365);

                var blobSasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobClient.BlobContainerName,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(expires.TotalSeconds)
                };

                blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
                var sas = blobClient.GenerateSasUri(blobSasBuilder);
                return sas.ToString();
            }
            catch
            {
                if (throwOnError) throw;
                return content.ToString(); // If creating blob failed then return same content to avoid data loss if used for logging!
            }
        }
    }
}