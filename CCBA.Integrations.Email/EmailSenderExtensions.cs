using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace CCBA.Integrations.Email
{
    public static class EmailSenderExtensions
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services, string fromAddress, string tenantId, string clientId, string clientSecret)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IEmailSender>(serviceProvider => new EmailSender(fromAddress, tenantId, clientId, clientSecret));

            return services;
        }

        public static IServiceCollection AddEmailSender(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IEmailSender>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var tenantId = configuration["TenantId"];
                var clientId = configuration["EmailSenderClientId"];
                var clientSecret = configuration["EmailSenderClientSecret"];
                var fromAddress = configuration["EmailSenderFromAddress"];

                return new EmailSender(fromAddress, tenantId, clientId, clientSecret);
            });

            return services;
        }
    }
}