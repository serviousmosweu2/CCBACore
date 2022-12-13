using Microsoft.Extensions.Configuration;

namespace CCBA.Integrations.Base.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class BaseConfiguration
    {
        protected readonly IConfiguration Configuration;

        protected BaseConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}