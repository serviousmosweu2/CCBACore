using System;

namespace CCBA.Integrations.Base.Models.HealthCheck
{
    public class HealthCheckConfigurationOptions
    {
        public HealthCheckConfigurationOptions(bool visible, bool optional = false, Type valueType = null)
        {
            Visible = visible;
            Optional = optional;
            ValueType = valueType;
        }

        /// <summary>
        /// Set to true for an optional checks where the status should not be aggregated toward a total overall health check
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Optional type to use when handling value eg. string, int, bool
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// Value will be shown in health check output
        /// </summary>
        public bool Visible { get; set; }
    }
}