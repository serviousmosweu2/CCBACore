using CCBA.Integrations.Base.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace CCBA.Integrations.Base.Models.HealthCheck
{
    public class HealthCheckResponseItem
    {
        public HealthCheckResponseItem()
        {
            Status = HealthCheckStatus.Ok;
            DateTime = DateTimeOffset.UtcNow;
        }

        public string Category { get; set; }

        public string Database { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public TimeSpan? Duration { get; set; }

        public string Endpoint { get; set; }

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(Name)) return null;
                return $"{Category}_{Name}".ToMd5Hex();
            }
        }

        public string Name { get; set; }

        public Dictionary<string, object> Properties { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HealthCheckStatus? Status { get; set; }

        public object Value { get; set; }
    }
}