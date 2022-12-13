using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;

namespace CCBA.Integrations.Base.Helpers
{
    public static class ServiceBusExtensions
    {
        public static T DeserializeObject<T>(this ServiceBusReceivedMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var deserializeObject = JsonConvert.DeserializeObject<T>(message.Body.ToString());
            if (deserializeObject == null) throw new NullReferenceException($"{nameof(deserializeObject)} is null");
            return deserializeObject;
        }
    }
}