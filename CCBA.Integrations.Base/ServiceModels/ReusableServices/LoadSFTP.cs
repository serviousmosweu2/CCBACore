using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Models;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public class LoadSFTP : Load<LoadSFTP.Input, bool>
    {
        public LoadSFTP(ILogger<LoadSFTP> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }

        protected override async Task<bool> Execute(Input input)
        {
            LogInformation($"Start FTP Host:{input.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);

            var buffer = Encoding.Default.GetBytes(input.Content);
            TempFileName = input.FileName;
            await buffer.SaveToBlobContainer(GetType().Name.ToLower(), input.FileName);
            await using var ms = new MemoryStream(buffer);
            int.TryParse(input.Port, out var port);
            new FTPAuth(input.Host, port, input.UserName, input.Password).Send(ms, input.Path + input.FileName);
            await CleanUp(TempFileName);
            LogInformation($"End FTP Host:{input.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);
            return true;
        }

        public class Input
        {
            public Input(string host, string userName, string password, string path, string port, string content, string fileName)
            {
                FileName = fileName;
                Host = host;
                UserName = userName;
                Password = password;
                Path = path;
                Port = port;
                Content = content;
            }

            public string Content { get; }
            public string FileName { get; }
            public string Host { get; }
            public string Password { get; }
            public string Path { get; }
            public string Port { get; }
            public string UserName { get; }
        }
    }
}