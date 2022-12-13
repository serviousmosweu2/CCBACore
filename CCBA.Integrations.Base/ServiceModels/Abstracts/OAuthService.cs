using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// Dependencies: <see cref="IMemoryCache"/>
    /// </summary>
    public class OAuthService : BaseLogger, IOAuthService
    {
        public string ApiBase;
        [NonSerialized] protected string ClientId;
        [NonSerialized] protected string Secret;
        [NonSerialized] protected string TenantId;

        private static readonly object _cacheLock = new();
        private readonly ILogger<OAuthService> _logger;
        private readonly IMemoryCache _memoryCache;

        public OAuthService(ILogger<OAuthService> logger, IConfiguration configuration, IMemoryCache memoryCache) : base(logger, configuration)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public string GetAccessToken()
        {
            return GetAccessTokenObjectAsync().GetAwaiter().GetResult().AccessToken;
        }

        public string GetCachedAccessToken()
        {
            lock (_cacheLock)
            {
                return _memoryCache.GetOrCreate($"{nameof(OAuthService)}_{nameof(GetCachedAccessToken)}_{ApiBase}_{TenantId}_{ClientId}", cacheEntry =>
                {
                    LogInformation($"Executing {nameof(GetCachedAccessToken)}", LogLevel.Trace, properties: new Dictionary<string, string>
                    {
                        { "ApiBase", ApiBase },
                        { "TenantId", TenantId },
                        { "ClientId", ClientId },
                    });

                    var token = GetAccessTokenObjectAsync().GetAwaiter().GetResult();

                    if (token == null || string.IsNullOrEmpty(token.AccessToken)) return null;

                    cacheEntry.Value = token.AccessToken;
                    cacheEntry.AbsoluteExpiration = token.ExpiresOn.AddMinutes(-1);

                    LogInformation($"Executed {nameof(GetCachedAccessToken)}", LogLevel.Trace, properties: new Dictionary<string, string>
                    {
                        { "ApiBase", ApiBase },
                        { "TenantId", TenantId },
                        { "ClientId", ClientId },
                    });

                    return token.AccessToken;
                });
            }
        }

        private async Task<AuthenticationResult> GetAccessTokenObjectAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var scopes = new List<string> { $"{ApiBase.Trim().TrimEnd('/')}/.default" };

            var confidentialClient = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(Secret)
                .WithAuthority(new Uri("https://login.microsoftonline.com/" + TenantId))
                .WithRedirectUri("http://localhost")
                .WithLogging(LoggingCallback)
                .Build();

            LogInformation($"Authenticating with {ApiBase}", properties: new Dictionary<string, string>
            {
                { "ApiBase", ApiBase },
                { "ClientId", $"{ClientId[..3]}xxxxx-xxxx-xxxx-xxxx-xxxxxxxxx{ClientId[^3..]}" },
                { "TenantId", $"{TenantId[..3]}xxxxx-xxxx-xxxx-xxxx-xxxxxxxxx{TenantId[^3..]}" },
                { "Duration", stopwatch.ElapsedMilliseconds.ToString() }
            });

            var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);
            var result = await accessTokenRequest.ExecuteAsync();
            return result;
        }

        private void LoggingCallback(Microsoft.Identity.Client.LogLevel level, string message, bool containsPii)
        {
            if (Configuration.GetValue("Logging:MSAL:Enabled", false))
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Trace, message);
        }
    }
}