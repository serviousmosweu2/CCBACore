using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class AisDataBaseAuthService : DataBaseAuthService
    {
        public AisDataBaseAuthService(ILogger<AisDataBaseAuthService> logger, IConfiguration configuration) : base(logger, configuration)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Configuration["AISDataSource"],
                InitialCatalog = Configuration["AISInitialCatalog"],
                UserID = Configuration["ManagedIdentityId"],
                Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                TrustServerCertificate = true,
                CommandTimeout = 30
            };

            ConnectionString = sqlConnectionStringBuilder.ConnectionString;

            sqlConnectionStringBuilder.Authentication = SqlAuthenticationMethod.ActiveDirectoryInteractive;
            
            LocalConnectionString = sqlConnectionStringBuilder.ConnectionString;
        }
    }
}