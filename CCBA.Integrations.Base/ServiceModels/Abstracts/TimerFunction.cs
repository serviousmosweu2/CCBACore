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
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public abstract class TimerFunction : AFunction
    {
        private readonly StopwatchService _stopWatchService;

        protected TimerFunction(ILogger<TimerFunction> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _stopWatchService = stopWatchService;
        }

        protected override async Task BaseRun(ExecutionContext context)
        {
            await base.BaseRun(context);

            try
            {
                AppStartLogger(context.InvocationId.ToString(), context.FunctionName, "N/A", Source, Target);
                var s = $"{GetType().BaseType?.Name.Replace("`2", "")} -> {GetType().Name}";
                _stopWatchService?.Start(s);
                await Main();
                _stopWatchService?.Stop(s);
                AppSuccessLogger(Source, Target);
            }
            catch (Exception exception)
            {
                AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                AppFailureLogger(Source, Target);
                if (!PartialSuccessAllowed)
                {
                    throw new Exception("Partial success not allowed!");
                }
            }
        }

        protected abstract Task Main();
    }
}