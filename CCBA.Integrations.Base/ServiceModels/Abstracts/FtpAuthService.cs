using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class FtpAuthService : BaseLogger
    {
        [NonSerialized] protected internal string Host;
        [NonSerialized] protected internal int Port;
        [NonSerialized] protected string Password;
        [NonSerialized] protected string UserName;

        protected FtpAuthService(ILogger<FtpAuthService> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(Host, Port, UserName, new PasswordAuthenticationMethod(UserName, Password));
        }
    }
}