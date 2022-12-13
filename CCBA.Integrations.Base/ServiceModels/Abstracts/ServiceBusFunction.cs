using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    [Obsolete]
    public abstract class ServiceBusFunction : AFunction
    {
        private readonly ExtractLegalEntity _extractLegalEntity;
        private readonly StopwatchService _stopWatchService;

        protected ServiceBusFunction(ILogger<ServiceBusFunction> logger, IConfiguration configuration, ExtractLegalEntity extractLegalEntity, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _extractLegalEntity = extractLegalEntity;
            _stopWatchService = stopWatchService;
        }

        protected async Task BaseRun(ExecutionContext context, Message message, Func<Dictionary<string, string>, string> getLegalEntityFromMessage)
        {
            await base.BaseRun(context);
            try
            {
                var value = Encoding.Default.GetString(message.Body);
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                var entityFromMessage = getLegalEntityFromMessage(result);
                if (FilterLegalEntity(entityFromMessage)) return;
                AppStartLogger(context.InvocationId.ToString(), context.FunctionName, entityFromMessage, Source, Target);
                var s = $"{GetType().BaseType?.Name.Replace("`2", "")} -> {GetType().Name}";
                _stopWatchService?.Start(s);
                await Main(result);
                _stopWatchService?.Stop(s);
                AppSuccessLogger(Source, Target);
            }
            catch (Exception exception)
            {
                AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                AppFailureLogger(Source, Target);
            }
        }

        protected abstract Task Main(Dictionary<string, string> message);

        private bool FilterLegalEntity(string message)
        {
            if (_extractLegalEntity.LegalEntities.Any(s => s.ToLower().Equals(message.ToLower()))) return false;
            LogInformation($@"Legal entity:{message} not enabled.");
            return true;
        }
    }
}