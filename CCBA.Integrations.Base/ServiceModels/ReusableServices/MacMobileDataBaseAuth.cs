using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class MacMobileDataBaseAuth : DataBaseAuthService
    {
        public MacMobileDataBaseAuth(ILogger<MacMobileDataBaseAuth> logger, IConfiguration configuration) : base(logger, configuration)
        {
            var server = Configuration["MacMobileServer"];
            var userId = Configuration["MacMobileDBUserID"];
            var password = Configuration["MacMobileDBPassword"];
            var dataBase = Configuration["MacMobileDatabase"];
            ConnectionString = $@"server={server};user id={userId};password={password};database={dataBase}";
            LocalConnectionString = ConnectionString;
        }
    }
}