namespace CCBA.Integrations.Authentication.Azure.Models
{
    public class OAuth2
    {
        public OAuth2(string clientId, string secret, string resourceUri, string tenantId)
        {
            ClientId = clientId;
            Secret = secret;
            ResourceUri = resourceUri;
            TenantId = tenantId;
        }

        public OAuth2()
        {
        }

        public string ClientId { get; set; }
        public string ResourceUri { get; set; }
        public string Secret { get; set; }
        public string TenantId { get; set; }
    }
}