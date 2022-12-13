using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class MacMobileFTPAuth : FtpAuthService
    {
        public MacMobileFTPAuth(ILogger<MacMobileFTPAuth> logger, IConfiguration configuration) : base(logger, configuration)
        {
            Port = 22;
            Host = Configuration["MacMobileServer"];
            UserName = Configuration["MacMobileFTPUserID"];
            Password = Configuration["MacMobileFTPPassword"];
        }
    }
}