using CCBA.Integrations.Base.Interfaces;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;

namespace CCBA.Integrations.Base.Helpers
{
    public static class ODataServiceExtensions
    {
        /// <summary>
        /// Add the ODataD365CeService to the specified services collection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">Calling assembly</param>
        /// <param name="oDataServiceOptions"></param>
        /// <param name="oAuthService"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddODataD365CeService(this IServiceCollection services, Assembly assembly, ODataServiceOptions oDataServiceOptions = null, IOAuthService oAuthService = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();

            var apiBase = string.IsNullOrWhiteSpace(configuration["D365CE:Url"]) ? configuration["D365CEUrl"] : configuration["D365CE:Url"];
            oDataServiceOptions ??= new ODataServiceOptions("D365CE", apiBase) { UseAuthorization = true };

            services.AddMemoryCache();
            services.AddHttpClient(oDataServiceOptions.ClientName);

            if (oAuthService != null)
            {
                services.TryAddTransient(_ => oAuthService);
            }
            else
            {
                services.TryAddTransient(serviceProvider =>
                    new OAuthD365CeService(serviceProvider.GetRequiredService<ILogger<OAuthD365CeService>>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    serviceProvider.GetRequiredService<IMemoryCache>(),
                    assembly.GetName()
                    ));
            }

            services.TryAddTransient(serviceProvider =>
                new ODataD365CeService(serviceProvider.GetRequiredService<ILogger<ODataD365CeService>>(),
                serviceProvider.GetRequiredService<IConfiguration>(),
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                oDataServiceOptions,
                serviceProvider.GetRequiredService<OAuthD365CeService>()));

            return services;
        }

        /// <summary>
        /// Add the ODataD365FoService to the specified services collection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">Calling assembly</param>
        /// <param name="oDataServiceOptions"></param>
        /// <param name="oAuthService"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddODataD365FoService(this IServiceCollection services, Assembly assembly, ODataServiceOptions oDataServiceOptions = null, IOAuthService oAuthService = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();

            var apiBase = string.IsNullOrWhiteSpace(configuration["D365FO:Url"]) ? configuration["D365FOUrl"] : configuration["D365FO:Url"];
            oDataServiceOptions ??= new ODataServiceOptions("D365FO", apiBase) { UseAuthorization = true };

            services.AddMemoryCache();
            services.AddHttpClient(oDataServiceOptions.ClientName);

            if (oAuthService != null)
            {
                services.TryAddTransient(_ => oAuthService);
            }
            else
            {
                services.TryAddTransient(serviceProvider =>
                    new OAuthD365FoService(serviceProvider.GetRequiredService<ILogger<OAuthD365FoService>>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    serviceProvider.GetRequiredService<IMemoryCache>(),
                    assembly.GetName()
                    ));
            }

            services.TryAddTransient(serviceProvider =>
                new ODataD365FoService(serviceProvider.GetRequiredService<ILogger<ODataD365FoService>>(),
                serviceProvider.GetRequiredService<IConfiguration>(),
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                oDataServiceOptions,
                serviceProvider.GetRequiredService<OAuthD365FoService>()));

            return services;
        }

        /// <summary>
        /// Add the ODataService to the specified services collection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="oDataServiceOptions"></param>
        /// <param name="oAuthService"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddODataService(this IServiceCollection services, ODataServiceOptions oDataServiceOptions = null, IOAuthService oAuthService = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            oDataServiceOptions ??= new ODataServiceOptions("ODataService", null) { UseAuthorization = false };

            services.AddMemoryCache();
            services.AddHttpClient(oDataServiceOptions.ClientName);

            services.TryAddTransient(serviceProvider =>
                new ODataService(serviceProvider.GetRequiredService<ILogger<ODataService>>(),
                serviceProvider.GetRequiredService<IConfiguration>(),
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                oDataServiceOptions,
                oAuthService));
            return services;
        }

        public static ODataServiceOptions GetODataD365CeServiceOptions(this IServiceCollection services)
        {
            return new ODataServiceOptions("D365CE", services.BuildServiceProvider().GetService<IConfiguration>()["D365CEUrl"]);
        }

        public static ODataServiceOptions GetODataD365FoServiceOptions(this IServiceCollection services)
        {
            return new ODataServiceOptions("D365FO", services.BuildServiceProvider().GetService<IConfiguration>()["D365FOUrl"]);
        }
    }
}