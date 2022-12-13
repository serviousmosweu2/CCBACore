using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class DataBaseAuthService : BaseLogger
    {
        protected DataBaseAuthService(ILogger<DataBaseAuthService> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public string ConnectionString { get; set; }
        public string DataBase { get; set; }
        public string LocalConnectionString { get; set; }
        public string Server { get; set; }
        public string UserId { get; set; }

        public string GetConnectionString()
        {
            return EnvironmentExtensions.IsDevelopment ? LocalConnectionString : ConnectionString;
        }
    }
}