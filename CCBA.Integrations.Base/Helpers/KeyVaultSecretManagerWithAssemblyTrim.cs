using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    ///  Developer: Konrad Steynberg
    /// </summary>
    public class KeyVaultSecretManagerWithAssemblyTrim : KeyVaultSecretManager
    {
        private readonly string _assembly;

        public KeyVaultSecretManagerWithAssemblyTrim(string assembly)
        {
            _assembly = assembly;
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            return secret.Name.Replace("--", ConfigurationPath.KeyDelimiter).Replace("-", ".").Replace($"{_assembly}:", string.Empty);
        }
    }
}