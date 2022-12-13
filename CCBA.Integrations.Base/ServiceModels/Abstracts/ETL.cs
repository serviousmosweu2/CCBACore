using Azure.Storage.Blobs;
using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public abstract class ETL<TInput, TOutput> : BaseLogger, IETL<TInput, TOutput>
    {
        private readonly StopwatchService _stopWatchService;

        protected ETL(ILogger<ETL<TInput, TOutput>> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _stopWatchService = stopWatchService;
        }

        public string Source { get; set; }
        public string Target { get; set; }
        public string TempFileName { get; set; }

        public async Task<TOutput> Run(TInput input)
        {
            var message = $"{GetType().BaseType?.Name.Replace("`2", "")} -> {GetType().Name}";
            try
            {
                LogInformation($"Starting: {message}", LogLevel.Information, Source, Target);

                _stopWatchService?.Start(message);
                var execute = await Execute(input);
                _stopWatchService?.Stop(message);
                LogInformation($"Ending: {message}", LogLevel.Information, Source, Target);
                return execute;
            }
            catch (Exception exception)
            {
                _stopWatchService?.Stop(message);
                LogInformation($"Error:{message} {exception.GetAllMessages()}", LogLevel.Error, Source, Target);
                throw;
            }
        }

        protected async Task CleanUp(string fileName)
        {
            try
            {
                var serviceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                var sourceContainerClient = serviceClient.GetBlobContainerClient(GetType().Name.ToLower());
                var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "archive");
                await blobContainerClient.CreateIfNotExistsAsync();
                var sourceBlobClient = sourceContainerClient.GetBlobClient(fileName);
                var targetBlobClient = blobContainerClient.GetBlobClient($@"{DateTime.Today:yyyy/MM/dd}/{fileName}");
                var result = await targetBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
                LogInformation($"Moved {fileName} to archive successful", LogLevel.Information, Source, Target);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            var bc = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), GetType().Name.ToLower());
            await bc.DeleteBlobIfExistsAsync(fileName);
        }

        protected abstract Task<TOutput> Execute(TInput input);
    }
}