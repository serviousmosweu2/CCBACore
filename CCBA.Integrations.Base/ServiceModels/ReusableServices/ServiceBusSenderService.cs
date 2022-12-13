using Azure.Messaging.ServiceBus;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public class ServiceBusSenderService : Load<ServiceBusSenderService.Input, ServiceBusSenderService.Output>
    {
        public ServiceBusSenderService(ILogger<ServiceBusSenderService> logger, IConfiguration configuration, StopwatchService stopwatchService) : base(logger, configuration, stopwatchService)
        {
        }

        protected override async Task<Output> Execute(Input input)
        {
            await SendMessageToServiceBus(input.Object, input.ConnectionKey, input.QueueName);
            return new Output(true);
        }

        private async Task SendMessageToServiceBus(string line, string kvtConnKey, string queueName)
        {
            await using var serviceBusClient = new ServiceBusClient(Configuration[kvtConnKey]);
            var serviceBusSender = serviceBusClient.CreateSender(queueName);
            var serviceBusMessage = new ServiceBusMessage(line);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }

        public class Input
        {
            public Input(object o, string connectionKey, string queueName)
            {
                ConnectionKey = connectionKey;
                QueueName = queueName;
                Object = JsonConvert.SerializeObject(o);
            }

            public Input(string o, string connectionKey, string queueName)
            {
                ConnectionKey = connectionKey;
                QueueName = queueName;
                Object = o;
            }

            public string ConnectionKey { get; }
            public string Object { get; set; }
            public string QueueName { get; }
        }

        public class Output
        {
            public Output(bool b)
            {
                Success = b;
            }

            public bool Success { get; }
        }
    }
}