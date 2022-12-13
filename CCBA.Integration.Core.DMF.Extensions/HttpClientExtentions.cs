using CCBA.Integration.Core.DMF.Extensions.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace CCBA.Integration.Core.DMF.Extensions
{
    public static class HttpClientExtentions
    {
        public static void AddD365HttpClient(this IServiceCollection services)
        {
            services.AddHttpClient("D365FOAuthorizedClient", (provider, client) =>
            {
                var settings = provider.GetService<IOptions<D365FOApplicationSettings>>();

                var authorityUri = $"https://login.microsoftonline.com/{settings.Value.TenantId}";
                var redirectUri = "http://localhost";
                var scopes = new List<string> { $"{settings.Value.ResourcesUri}/.default" };

                var confidentialClient = ConfidentialClientApplicationBuilder.Create(settings.Value.ClientId)
                       .WithClientSecret(settings.Value.Secret)
                       .WithAuthority(new Uri(authorityUri))
                       .WithRedirectUri(redirectUri)
                       .Build();
                var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

                var token = accessTokenRequest.ExecuteAsync().GetAwaiter().GetResult().AccessToken;

                client.BaseAddress = new Uri(settings.Value.ResourcesUri);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            });
        }
    }
}
