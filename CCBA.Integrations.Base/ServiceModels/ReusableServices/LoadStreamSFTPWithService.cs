using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="FtpAuthService"/>
    /// </summary>
    public class LoadStreamSFTPWithService : Load<LoadStreamSFTPWithService.Input, bool>
    {
        private readonly FtpAuthService _ftpAuth;

        public LoadStreamSFTPWithService(ILogger<LoadStreamSFTPWithService> logger, IConfiguration configuration, FtpAuthService ftpAuth, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
            _ftpAuth = ftpAuth;
        }

        protected override async Task<bool> Execute(Input input)
        {
            LogInformation($"Start FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);

            TempFileName = input.FileName;
            input.Content.Position = 0;
            await input.Content.ToArray().SaveToBlobContainer(GetType().Name.ToLower(), input.FileName);

            input.Content.Position = 0;

            Policy.Handle<Exception>().WaitAndRetry(15, attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt))).Execute(() =>
            {
                _ftpAuth.Send(input.Content, input.Path + input.FileName);
            });
            await CleanUp(TempFileName);
            LogInformation($"End FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);
            return true;
        }

        public class Input
        {
            public Input(string path, MemoryStream content, string fileName)
            {
                FileName = fileName;
                Path = path;
                Content = content;
            }

            public MemoryStream Content { get; }
            public string FileName { get; }
            public string Path { get; }
        }
    }
}