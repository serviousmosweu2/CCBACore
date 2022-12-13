using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CCBA.Integrations.Base.Models.HealthCheck
{
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public class HealthCheckResponse
    {
        public string Assembly { get; set; }

        public List<HealthCheckResponseItem> Configuration { get; set; }

        public Dictionary<string, string> DomainAssemblies { get; set; }

        public List<HealthCheckResponseItem> Dynamics { get; set; }

        public List<HealthCheckResponseItem> Endpoints { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HealthCheckStatus Status { get; set; }

        public List<HealthCheckResponseItem> Synapse { get; set; }
    }
}