using Azure.Identity;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Email
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Dattatray Mharanur
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _fromEmail;
        private readonly string _tenantId;

        public EmailSender(string fromEmail, string tenantId, string clientId, string clientSecret)
        {
            _fromEmail = fromEmail;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task SendEmailAsyncUsingGraph(IEnumerable<string> toEmailAddresses, string subject, string messageBody, bool isHtml, Importance mailPriority, MessageAttachmentsCollectionPage attachments = null)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
            var clientSecretCredential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret, options);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var toRecipients = toEmailAddresses.Select(toEmail => new Recipient { EmailAddress = new EmailAddress { Address = toEmail } }).ToList();

            var message = new Message
            {
                Subject = subject,
                Importance = mailPriority,
                Body = new ItemBody
                {
                    ContentType = isHtml ? BodyType.Html : BodyType.Text,
                    Content = messageBody
                },
                ToRecipients = toRecipients,
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = _fromEmail
                    }
                }
            };
            if (attachments != null && attachments.Any()) message.Attachments = attachments;

            await graphClient.Users[_fromEmail]
                .SendMail(message, false)
                .Request()
                .PostAsync();
        }
    }
}