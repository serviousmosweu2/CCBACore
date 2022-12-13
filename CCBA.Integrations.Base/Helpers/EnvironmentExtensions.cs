using System;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// </summary>
    public static class EnvironmentExtensions
    {
        public static bool IsDevelopment => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development";
    }
}