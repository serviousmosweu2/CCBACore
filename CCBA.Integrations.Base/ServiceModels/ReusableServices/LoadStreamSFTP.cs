using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Models;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public class LoadStreamSFTP : Load<LoadStreamSFTP.Input, bool>
    {
        public LoadStreamSFTP(ILogger<LoadStreamSFTP> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }

        protected override async Task<bool> Execute(Input input)
        {
            LogInformation($"Start FTP Host:{input.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);

            TempFileName = input.FileName;
            input.Content.Position = 0;
            await input.Content.ToArray().SaveToBlobContainer(GetType().Name.ToLower(), input.FileName);
            int.TryParse(input.Port, out var port);
            input.Content.Position = 0;
            new FTPAuth(input.Host, port, input.UserName, input.Password).Send(input.Content, input.Path + input.FileName);
            await CleanUp(TempFileName);
            LogInformation($"End FTP Host:{input.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);
            return true;
        }

        public class Input
        {
            public Input(string host, string userName, string password, string path, string port, MemoryStream content, string fileName)
            {
                FileName = fileName;
                Host = host;
                UserName = userName;
                Password = password;
                Path = path;
                Port = port;
                Content = content;
            }

            public MemoryStream Content { get; }
            public string FileName { get; }
            public string Host { get; }
            public string Password { get; }
            public string Path { get; }
            public string Port { get; }
            public string UserName { get; }
        }
    }
}