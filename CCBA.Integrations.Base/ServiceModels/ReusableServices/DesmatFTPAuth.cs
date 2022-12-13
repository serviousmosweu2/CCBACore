using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class DesmatFTPAuth : FtpAuthService
    {
        public DesmatFTPAuth(ILogger<DesmatFTPAuth> logger, IConfiguration configuration) : base(logger, configuration)
        {
            int.TryParse(Configuration["DesmatFtpPort"], out var port);
            Port = port;
            Host = Configuration["DesmatFtpHostName"];
            UserName = Configuration["DesmatFtpUserName"];
            Password = Configuration["DesmatFtpPassword"];
        }
    }
}