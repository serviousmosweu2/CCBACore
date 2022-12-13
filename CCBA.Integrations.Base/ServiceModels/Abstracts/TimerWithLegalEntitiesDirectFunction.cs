using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="ExtractLegalEntity"/>
    /// </summary>
    public abstract class TimerWithLegalEntitiesDirectFunction : AFunction
    {
        private readonly ExtractLegalEntity _extractLegalEntity;
        private readonly StopwatchService _stopWatchService;

        protected TimerWithLegalEntitiesDirectFunction(ILogger<TimerWithLegalEntitiesDirectFunction> logger, IConfiguration configuration, ExtractLegalEntity extractLegalEntity, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _stopWatchService = stopWatchService;
            _extractLegalEntity = extractLegalEntity;
        }

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
        }

        protected abstract Task Main(string legalEntity);
    }
}