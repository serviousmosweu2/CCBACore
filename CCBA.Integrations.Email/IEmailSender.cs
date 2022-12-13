using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integrations.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsyncUsingGraph(IEnumerable<string> toEmailAddresses, string subject, string message, bool isHtml, Importance mailPriority, MessageAttachmentsCollectionPage attachments = null);
    }
}