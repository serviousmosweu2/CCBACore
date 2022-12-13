using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Konrad Steynberg
    /// </summary>
    public class ServiceBusMessageLockHelper : IDisposable
    {
        public ServiceBusMessageLockHelper(Message message, MessageReceiver messageReceiver, CancellationToken cancellationToken, ILogger logger = null)
        {
            if (messageReceiver == null) return;

            // If we lose the lock we should trigger a cancellation.
            // This will allow long running processes to restart processing when the message is retrieved from the queue again.
            // We can create a CancellationTokenSource which has a Cancel method
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5).AddJitter(), source.Token);

                while (RenewLockEnabled && !source.Token.IsCancellationRequested)
                {
                    try
                    {
                        await messageReceiver.RenewLockAsync(message.SystemProperties.LockToken);
                        logger?.LogTrace("ServiceBus message lock renewed. MessageId={MessageId}", message.MessageId); // Log a trace message for visibility
                        await Task.Delay(RenewLockInterval.AddJitter(), source.Token);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("expired"))
                        {
                            RenewLockEnabled = false;
                            source.Cancel(false); // Trigger cancellation
                        }
                    }
                }
            }, source.Token);
        }

        public bool RenewLockEnabled { get; set; } = true;

        public TimeSpan RenewLockInterval { get; set; } = TimeSpan.FromSeconds(30);

        public void Dispose()
        {
            try
            {
                RenewLockEnabled = false;
            }
            catch (Exception e)
            {
                // ignore
            }
        }
    }
}