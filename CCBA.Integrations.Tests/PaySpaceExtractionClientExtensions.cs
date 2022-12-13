using CCBA.Integrations.Base.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace CCBA.Integrations.Tests
{
    public static class PaySpaceExtractionClientExtensions
    {
        public static IServiceCollection AddPaySpaceExtractionClient(this IServiceCollection services, PaySpaceExtractionClientOptions oDataServiceOptions = null, IOAuthService oAuthService = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            oDataServiceOptions ??= new PaySpaceExtractionClientOptions("PaySpaceExtractionClient", null) { RetryDelay = 1000 };

            services.AddHttpClient(oDataServiceOptions.ClientName);

            services.TryAddTransient(serviceProvider =>
                new PaySpaceExtractionClient(serviceProvider.GetRequiredService<ILogger<PaySpaceExtractionClient>>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    oDataServiceOptions,
                    oAuthService));

            return services;
        }
    }
}