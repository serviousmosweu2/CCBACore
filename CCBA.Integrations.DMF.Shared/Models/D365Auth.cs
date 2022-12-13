using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integrations.DMF.Shared.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public sealed class D365Auth
    {
        public readonly string ApiBase;

        [NonSerialized]
        private readonly string _clientId;

        [NonSerialized]
        private readonly string _secret;

        [NonSerialized]
        private readonly string _tenantId;

        public D365Auth(string apiBase, string clientId, string tenantId, string secret)
        {
            ApiBase = apiBase;
            _clientId = clientId;
            _tenantId = tenantId;
            _secret = secret;
        }

        public async Task<string> GetAuthTokenAsync()
        {
            var scopes = new List<string> { $"{ApiBase}/.default" };

            var confidentialClient = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithClientSecret(_secret)
                .WithAuthority(new Uri("https://login.microsoftonline.com/" + _tenantId))
                .WithRedirectUri("http://localhost")
                .Build();
            var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

            return (await accessTokenRequest.ExecuteAsync()).AccessToken;
        }
    }
}