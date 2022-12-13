using Microsoft.Extensions.Configuration;
using System;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static void CheckConfiguration(this IConfiguration configuration, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(configuration[key]))
                {
                    throw new Exception($@"{key} value is missing!");
                }
            }
        }
    }
}