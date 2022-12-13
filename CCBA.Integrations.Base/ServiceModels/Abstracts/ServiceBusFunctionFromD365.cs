using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="ExtractLegalEntity"/>
    /// </summary>
    public abstract class ServiceBusFunctionFromD365 : AFunction
    {
        protected Message Message;
        protected MessageReceiver MessageActions;

        private readonly ExtractLegalEntity _extractLegalEntity;
        private readonly ILogger<ServiceBusFunctionFromD365> _logger;
        private readonly StopwatchService _stopWatchService;

        protected ServiceBusFunctionFromD365(ILogger<ServiceBusFunctionFromD365> logger, IConfiguration configuration, ExtractLegalEntity extractLegalEntity, StopwatchService stopWatchService) : base(logger, configuration)
        {
            _logger = logger;
            _extractLegalEntity = extractLegalEntity;
            _stopWatchService = stopWatchService;
        }

        protected async Task BaseRun(ExecutionContext context, Message message, Func<Dictionary<string, string>, string> getLegalEntityFromMessage, CancellationToken cancellationToken)
        {
            await base.BaseRun(context);
            try
            {
                if (IsBusinessEventsTestEndpointContract(message))
                {
                    // When business events are configured, a test event of type BusinessEventsTestEndpointContract is produced.
                    // We can handle this message automatically and log that it was handled correctly.
                    if (MessageActions != null) await MessageActions.CompleteAsync(Message.SystemProperties.LockToken);

                    LogInformation($"Message completed (BusinessEventsTestEndpointContract)", properties: new Dictionary<string, string>
                    {
                        { "MessageId", message.MessageId },
                        { "CorrelationId", message.CorrelationId }
                    });
                    return;
                }

                var value = Encoding.Default.GetString(message.Body);
                var result = JsonExtensions.DeserializeAndFlatten(value).ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
                var entityFromMessage = getLegalEntityFromMessage(result);
                if (FilterLegalEntity(entityFromMessage)) return;
                AppStartLogger(context.InvocationId.ToString(), context.FunctionName, entityFromMessage, Source, Target);
                var s = $"{GetType().BaseType?.Name.Replace("`2", "")} -> {GetType().Name}";
                _stopWatchService?.Start(s);

                using (new ServiceBusMessageLockHelper(message, MessageActions, cancellationToken, _logger))
                {
                    await Main(result);
                }
                _stopWatchService?.Stop(s);
                AppSuccessLogger(Source, Target);
                if (MessageActions != null)
                {
                    try
                    {
                        await MessageActions.CompleteAsync(Message.SystemProperties.LockToken);
                        LogInformation("Message completed", properties: new Dictionary<string, string>
                        {
                            { "MessageId", message.MessageId },
                            { "CorrelationId", message.CorrelationId }
                        });
                    }
                    catch (Exception e)
                    {
                        LogException(e.Message, e, LogLevel.Warning); // This is not thrown again to avoid trying to dead-letter a message when the lock is no longer valid
                    }
                }
                else
                {
                    LogInformation("MessageActions not set. Complete not run.", LogLevel.Warning, Source, Target);
                }
            }
            catch (Exception exception)
            {
                if (MessageActions != null)
                {
                    var deadLetterReason = exception.Message;
                    if (deadLetterReason.Length > 4096) deadLetterReason = deadLetterReason[..4096];

                    var deadLetterDescription = exception.ToString();
                    if (deadLetterDescription.Length > 4096) deadLetterDescription = deadLetterDescription[..4096];

                    await MessageActions.DeadLetterAsync(Message.SystemProperties.LockToken, deadLetterReason, deadLetterDescription);
                }
                else
                {
                    LogInformation(@"MessageActions not set. Message not dead-lettered.", LogLevel.Warning, Source, Target);
                    LogInformation("Message dead-lettered", LogLevel.Warning, properties: new Dictionary<string, string>
                    {
                        { "MessageId", message.MessageId },
                        { "CorrelationId", message.CorrelationId }
                    });
                }

                AppExceptionLogger($@"Something went wrong! {exception.GetAllMessages()}", EErrorCode.Other, LogLevel.Critical, exception, Source, Target);
                AppFailureLogger(Source, Target);
            }
        }

        protected abstract Task Main(Dictionary<string, string> message);

        private bool FilterLegalEntity(string message)
        {
            if (_extractLegalEntity.LegalEntities.Any(s => s.ToLower().Equals(message.ToLower()))) return false;
            LogInformation($@"Legal entity:{message} not enabled.", LogLevel.Warning, Source, Target);
            return true;
        }

        private bool IsBusinessEventsTestEndpointContract(Message message)
        {
            var isTest = false;
            try
            {
                var json = BinaryData.FromBytes(message.Body).ToString();
                var jObject = JObject.Parse(json);
                if (jObject["BusinessEventId"].ToString() == "BusinessEventsTestEndpointContract") isTest = true;
            }
            catch
            {
                // ignored
            }

            return isTest;
        }
    }
}