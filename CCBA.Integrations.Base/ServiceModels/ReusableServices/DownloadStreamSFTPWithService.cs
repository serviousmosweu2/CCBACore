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
    /// Dependencies: <see cref="FtpAuthService"/>, <see cref="StopwatchService"/>
    /// </summary>
    public class DownloadStreamSFTPWithService : Load<DownloadStreamSFTPWithService.Input, bool>
    {
        private readonly FtpAuthService _ftpAuth;

        public DownloadStreamSFTPWithService(ILogger<DownloadStreamSFTPWithService> logger, IConfiguration configuration, FtpAuthService ftpAuth, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
            _ftpAuth = ftpAuth;
        }

        protected override async Task<bool> Execute(Input input)
        {
            LogInformation($"Start FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);

            TempFileName = input.FileName;
            input.Content.Position = 0;

            await Policy.Handle<Exception>().WaitAndRetry(15, attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt))).Execute(async () =>
             {
                 _ftpAuth.DownloadFile(input.Content, input.Path + input.FileName);

                 await input.Content.SaveToBlobContainer(GetType().Name.ToLower(), input.FileName);
             });
            LogInformation($"End FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);
            return true;
        }

        public class Input
        {
            public Input(MemoryStream content, string path, string fileName)
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