using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class FandODataBaseAuthService : DataBaseAuthService
    {
        public FandODataBaseAuthService(ILogger<FandODataBaseAuthService> logger, IConfiguration configuration) : base(logger, configuration)
        {
            Server = string.IsNullOrWhiteSpace(Configuration["FO:DataLake:DataSource"]) ? Configuration["FODataLakeDataSource"] : Configuration["FO:DataLake:DataSource"];
            DataBase = string.IsNullOrWhiteSpace(Configuration["FO:DataLake:InitialCatalog"]) ? Configuration["FODataLakeInitialCatalog"] : Configuration["FO:DataLake:InitialCatalog"];
            UserId = Configuration["ManagedIdentityId"];
            ConnectionString = $@"Data Source={Server};Initial Catalog={DataBase};User ID={UserId};Authentication=ActiveDirectoryManagedIdentity;TrustServerCertificate=True;Command Timeout=1680";
            LocalConnectionString = $@"Data Source={Server};Initial Catalog={DataBase};User ID={UserId};Authentication=ActiveDirectoryInteractive;TrustServerCertificate=True;Command Timeout=1680";
        }
    }
}