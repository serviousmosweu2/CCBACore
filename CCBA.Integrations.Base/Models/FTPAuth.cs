using Renci.SshNet;
using System;

namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class FTPAuth
    {
        [NonSerialized] private readonly string _host;
        [NonSerialized] private readonly string _password;
        [NonSerialized] private readonly int _port;
        [NonSerialized] private readonly string _userName;

        public FTPAuth(string host, int port, string userName, string password)
        {
            _host = host;
            _port = port;
            _userName = userName;
            _password = password;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(_host, _port, _userName, new PasswordAuthenticationMethod(_userName, _password));
        }
    }
}