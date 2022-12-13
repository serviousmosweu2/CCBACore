using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace CCBA.Integrations.Base.Models.HealthCheck
{
    public class HealthCheckHttpEndpoint : HealthCheckEndpoint
    {
        public string Address { get; set; }
        public AuthenticationType Authentication { get; set; } = new AuthenticationTypeNone();
        public HttpMethod Method { get; set; }
        public List<HttpStatusCode> StatusCodeWhitelist { get; set; }
        public HealthCheckEndpointType Type => HealthCheckEndpointType.Tcp;

        public abstract class AuthenticationType
        {
        }

        public class AuthenticationTypeBasic : AuthenticationType
        {
            public string Password { get; set; }
            public string Username { get; set; }
        }

        public class AuthenticationTypeNone : AuthenticationType
        {
        }

        // TODO: implement bearer authentication
        /*public class AuthenticationTypeBearer : AuthenticationType
        {
            public Func<string> GetToken { get; set; }
        }*/
    }
}