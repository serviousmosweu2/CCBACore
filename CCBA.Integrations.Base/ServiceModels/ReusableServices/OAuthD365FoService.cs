using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Konrad Steynberg
    /// Dependencies: <see cref="IMemoryCache"/>
    /// </summary>
    public sealed class OAuthD365FoService : OAuthService
    {
        public OAuthD365FoService(ILogger<OAuthD365FoService> logger, IConfiguration configuration, IMemoryCache memoryCache, AssemblyName callingAssembly = null) : base(logger, configuration, memoryCache)
        {
            var assembly = callingAssembly != null ? callingAssembly.Name : GetType().Assembly.GetName().Name;

            LogInformation($"{GetType().Name} using KeyVault prefix {assembly}");

            ClientId = Configuration["D365FO:ClientId"];

            if (string.IsNullOrEmpty(ClientId))
            {
                LogInformation($"{GetType().Name} secret not found: {assembly}:D365FO:ClientId");
                LogInformation($"{GetType().Name} using fallback: D365FOClientId");
                ClientId = Configuration["D365FOClientId"];
            }

            Secret = string.IsNullOrWhiteSpace(Configuration["D365FO:ClientSecret"]) ? Configuration["D365FOClientSecret"] : Configuration["D365FO:ClientSecret"];

            if (string.IsNullOrEmpty(Secret))
            {
                LogInformation($"{GetType().Name} secret not found: {assembly}:D365FO:ClientSecret");
                LogInformation($"{GetType().Name} using fallback: D365FOClientSecret");
                ClientId = Configuration["D365FOClientId"];
            }

            TenantId = Configuration["TenantId"];

            ApiBase = string.IsNullOrWhiteSpace(Configuration["D365FO:Url"]) ? Configuration["D365FOUrl"] : Configuration["D365FO:Url"];
        }
    }
}