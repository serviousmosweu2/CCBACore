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
    public sealed class OAuthD365CeService : OAuthService
    {
        public OAuthD365CeService(ILogger<OAuthD365CeService> logger, IConfiguration configuration, IMemoryCache memoryCache, AssemblyName callingAssembly = null) : base(logger, configuration, memoryCache)
        {
            var assembly = callingAssembly != null ? callingAssembly.Name : GetType().Assembly.GetName().Name;

            LogInformation($"{GetType().Name} using KeyVault prefix {assembly}");

            ClientId = Configuration["D365CE:ClientId"];

            if (string.IsNullOrEmpty(ClientId))
            {
                LogInformation($"{GetType().Name} secret not found: {assembly}:D365CE:ClientId");
                LogInformation($"{GetType().Name} using fallback: D365CEClientId");
                ClientId = Configuration["D365CEClientId"];
            }

            Secret = string.IsNullOrWhiteSpace(Configuration["D365CE:ClientSecret"]) ? Configuration["D365CEClientSecret"] : Configuration["D365CE:ClientSecret"];

            if (string.IsNullOrEmpty(Secret))
            {
                LogInformation($"{GetType().Name} secret not found: {assembly}:D365CE:ClientSecret");
                LogInformation($"{GetType().Name} using fallback: D365CEClientSecret");
                ClientId = Configuration["D365CEClientId"];
            }

            TenantId = Configuration["TenantId"];

            ApiBase = string.IsNullOrWhiteSpace(Configuration["D365CE:Url"]) ? Configuration["D365CEUrl"] : Configuration["D365CE:Url"];
        }
    }
}