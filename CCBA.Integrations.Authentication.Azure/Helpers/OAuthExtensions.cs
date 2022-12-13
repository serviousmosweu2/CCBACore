using CCBA.Integrations.Authentication.Azure.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integrations.Authentication.Azure.Helpers
{
    /// <summary>
    /// Developer:  Johan Nieuwenhuis
    /// </summary>
    public static class OAuthExtensions
    {
        private const string RedirectUri = "http://localhost";
        private static string _authorityUri = "https://login.microsoftonline.com/";

        /// <summary>
        /// Retrieves an authentication header from the service.
        /// </summary>
        /// <returns>The authentication header for the Web API call.</returns>
        public static async Task<string> GetAuthTokenAsync(this OAuth2 oAuth2)
        {
            var scopes = new List<string> { $"{oAuth2.ResourceUri}/.default" };

            var confidentialClient = ConfidentialClientApplicationBuilder.Create(oAuth2.ClientId)
                .WithClientSecret(oAuth2.Secret)
                .WithAuthority(new Uri(_authorityUri + oAuth2.TenantId))
                .WithRedirectUri(RedirectUri)
                .Build();
            var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

            return (await accessTokenRequest.ExecuteAsync()).AccessToken;
        }

        /// <summary>
        /// Retrieves an authentication header from the service.
        /// </summary>
        /// <returns>The authentication header for the Web API call.</returns>
        public static async Task<string> GetAuthTokenAsync(string clientId, string secret, string resourceUri, string tenantId)
        {
            var scopes = new List<string> { $"{resourceUri}/.default" };

            var confidentialClient = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(secret)
                .WithAuthority(new Uri(_authorityUri + tenantId))
                .WithRedirectUri("http://localhost")
                .Build();
            var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

            return (await accessTokenRequest.ExecuteAsync()).AccessToken;
        }
    }
}