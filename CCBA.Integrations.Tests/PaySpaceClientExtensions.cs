using CCBA.Integrations.Base.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace CCBA.Integrations.Tests
{
    public static class PaySpaceClientExtensions
    {
        public static IServiceCollection AddPaySpaceClient(this IServiceCollection services, PaySpaceClientOptions oDataServiceOptions = null, IOAuthService oAuthService = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();

            oDataServiceOptions ??= new PaySpaceClientOptions("PaySpaceClient", null) { RetryDelay = configuration.GetValue("PaySpaceExponentialBackOff", 1) * 100 };

            services.AddHttpClient(oDataServiceOptions.ClientName);

            services.TryAddTransient(serviceProvider =>
                new PaySpaceClient(serviceProvider.GetRequiredService<ILogger<PaySpaceClient>>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    oDataServiceOptions,
                    oAuthService));

            return services;
        }
    }
}