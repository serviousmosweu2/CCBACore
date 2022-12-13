using CCBA.Integrations.Base.Abstracts;
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
    /// </summary>
    public abstract class AFunction : BaseLogger
    {
        protected AFunction(ILogger<AFunction> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public bool PartialSuccessAllowed { get; set; }
        protected string Source { get; set; }

        protected string Target { get; set; }

        protected virtual async Task BaseRun(ExecutionContext context)
        {
            try
            {
                await Task.Delay(1);
                AppStartLogger(context.InvocationId.ToString(), context.FunctionName, "Init", Source, Target);
                Init();
            }
            catch (Exception exception)
            {
                AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                AppFailureLogger(Source, Target);
            }
        }

        protected abstract void Init();
    }
}