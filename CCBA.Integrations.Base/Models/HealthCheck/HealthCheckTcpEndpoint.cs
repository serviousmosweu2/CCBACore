namespace CCBA.Integrations.Base.Models.HealthCheck
{
    public class HealthCheckTcpEndpoint : HealthCheckEndpoint
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public HealthCheckEndpointType Type => HealthCheckEndpointType.Http;
    }
}