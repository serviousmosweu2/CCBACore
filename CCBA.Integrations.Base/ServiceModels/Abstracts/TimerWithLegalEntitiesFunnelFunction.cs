using Azure.Storage.Blobs;
using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="ExtractLegalEntity"/>
    /// </summary>
    public abstract class TimerWithLegalEntitiesFunnelFunction : AFunction
    {
        private readonly ExtractLegalEntity _extractLegalEntity;
        private readonly StopwatchService _stopWatchService;

        protected TimerWithLegalEntitiesFunnelFunction(ILogger<TimerWithLegalEntitiesFunnelFunction> logger, IConfiguration configuration, ExtractLegalEntity extractLegalEntity, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _stopWatchService = stopWatchService;
            _extractLegalEntity = extractLegalEntity;
        }

        protected List<string> LoadFunnel { get; set; } = new List<string>();

        protected override async Task BaseRun(ExecutionContext context)
        {
            await base.BaseRun(context);

            foreach (var legalEntity in _extractLegalEntity.LegalEntities)
            {
                try
                {
                    AppStartLogger(context.InvocationId.ToString(), context.FunctionName, legalEntity, Source, Target);
                    var s = $"{GetType().BaseType?.Name.Replace("`2", "")} -> {GetType().Name}";
                    _stopWatchService?.Start(s);
                    await Main(legalEntity);
                    _stopWatchService?.Stop(s);
                    AppSuccessLogger(Source, Target);
                }
                catch (Exception exception)
                {
                    AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                    AppFailureLogger(Source, Target);
                }
            }
            await Finalize(context);
        }

        protected async Task CleanUp(string fileName)
        {
            var bc = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), GetType().Name.ToLower());
            await bc.DeleteBlobIfExistsAsync(fileName);
        }

        protected abstract Task Main(string legalEntity);

        protected abstract Task Upload();

        private async Task Finalize(ExecutionContext context)
        {
            if (!LoadFunnel.Any())
            {
                LogInformation("No Records Processed.", LogLevel.Information, Source, Target);
                return;
            }

            AppStartLogger(context.InvocationId.ToString(), context.FunctionName, "");

            try
            {
                await Upload();

                AppSuccessLogger(Source, Target);
            }
            catch (Exception exception)
            {
                AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                AppFailureLogger(Source, Target);
            }
        }
    }
}