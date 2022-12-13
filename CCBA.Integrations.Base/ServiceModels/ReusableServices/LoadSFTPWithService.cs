using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="FtpAuthService"/>
    /// </summary>
    public class LoadSFTPWithService : Load<LoadSFTPWithService.Input, bool>
    {
        private readonly FtpAuthService _ftpAuth;

        public LoadSFTPWithService(ILogger<LoadSFTPWithService> logger, IConfiguration configuration, FtpAuthService ftpAuth, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
            _ftpAuth = ftpAuth;
        }

        protected override async Task<bool> Execute(Input input)
        {
            LogInformation($"Start FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);

            var buffer = Encoding.Default.GetBytes(input.Content);
            TempFileName = input.FileName;
            await buffer.SaveToBlobContainer(GetType().Name.ToLower(), input.FileName);

            Policy.Handle<Exception>().WaitAndRetry(15, attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)), (exception, calculatedWaitDuration) =>
            {
                LogInformation(exception.GetAllMessages(), LogLevel.Error, Source, Target);
            }).Execute(() =>
            {
                using var ms = new MemoryStream(buffer);
                _ftpAuth.Send(ms, input.Path + input.FileName);
            });
            await CleanUp(TempFileName);
            LogInformation($"End FTP Host:{_ftpAuth.Host} Path:{input.Path} File:{input.FileName} ", LogLevel.Information, Source, Target);
            return true;
        }

        public class Input
        {
            public Input(string path, string content, string fileName)
            {
                FileName = fileName;
                Path = path;
                Content = content;
            }

            public string Content { get; }
            public string FileName { get; }
            public string Path { get; }
        }
    }
}