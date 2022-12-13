using CCBA.Integrations.Base.Models;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Renci.SshNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public static class SFTP
    {
        public static void DeleteFile(this FtpAuthService auth, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.DeleteFile(fileName);
            sftpClient.Dispose();
        }

        public static void DeleteFile(this FTPAuth auth, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.DeleteFile(fileName);
            sftpClient.Dispose();
        }

        public static void DownloadFile(this FtpAuthService auth, MemoryStream ms, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.DownloadFile($"{fileName}", ms);
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.Dispose();
        }

        public static async Task<byte[]> DownloadFile(this FtpAuthService auth, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            await using var ms = new MemoryStream();
            sftpClient.DownloadFile($"{fileName}", ms);
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.Dispose();
            return ms.ToArray();
        }

        public static void DownloadFile(this FTPAuth auth, MemoryStream ms, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.DownloadFile($"{fileName}", ms);
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.Dispose();
        }

        public static async Task<byte[]> DownloadFile(this FTPAuth auth, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            await using var ms = new MemoryStream();
            sftpClient.DownloadFile($"{fileName}", ms);
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.Dispose();
            return ms.ToArray();
        }

        public static List<string> GetFiles(this FtpAuthService auth, string path)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            var sftpFiles = sftpClient.ListDirectory(path).Select(s => s.FullName).ToList();
            sftpClient.Dispose();
            return sftpFiles;
        }

        public static List<string> GetFiles(this FTPAuth auth, string path)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            var sftpFiles = sftpClient.ListDirectory(path).Select(s => s.FullName).ToList();
            sftpClient.Dispose();
            return sftpFiles;
        }

        public static void Send(this FtpAuthService auth, MemoryStream ms, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.UploadFile(ms, $"{fileName}");
            sftpClient.Dispose();
        }

        public static async Task Send(this FtpAuthService auth, byte[] bytes, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            await using (var ms = new MemoryStream(bytes))
            {
                ms.Seek(0, SeekOrigin.Begin);
                sftpClient.UploadFile(ms, $"{fileName}");
            }

            sftpClient.Dispose();
        }

        public static void Send(this FTPAuth auth, MemoryStream ms, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            ms.Seek(0, SeekOrigin.Begin);
            sftpClient.UploadFile(ms, $"{fileName}");
            sftpClient.Dispose();
        }

        public static async Task Send(this FTPAuth auth, byte[] bytes, string fileName)
        {
            var connectionInfo = auth.GetConnectionInfo();
            using var sftpClient = new SftpClient(connectionInfo);
            sftpClient.Connect();
            sftpClient.BufferSize = 1024;
            await using (var ms = new MemoryStream(bytes))
            {
                ms.Seek(0, SeekOrigin.Begin);
                sftpClient.UploadFile(ms, $"{fileName}");
            }

            sftpClient.Dispose();
        }
    }
}