using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class CEDataBaseAuthService : DataBaseAuthService
    {
        public CEDataBaseAuthService(ILogger<CEDataBaseAuthService> logger, IConfiguration configuration) : base(logger, configuration)
        {
            Server = Configuration["CEDataLakeDataSource"];
            DataBase = Configuration["CEDataLakeInitialCatalog"];
            UserId = Configuration["ManagedIdentityId"];
            ConnectionString = $@"Data Source={Server};Initial Catalog={DataBase};User ID={UserId};Authentication=ActiveDirectoryManagedIdentity;TrustServerCertificate=True;Command Timeout=1680";
            LocalConnectionString = $@"Data Source={Server};Initial Catalog={DataBase};User ID={UserId};Authentication=ActiveDirectoryInteractive;TrustServerCertificate=True;Command Timeout=1680";
        }
    }
}