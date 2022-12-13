using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;

namespace CCBA.Integrations.Testing
{
    public abstract class TestBaseWithConfiguration : TestBase
    {
        protected override void Setup()
        {
            base.Setup();

            _configuration = _configurationBuilder.Build();

            // Add AppConfiguration - auth is default azure credential, therefore your VS must be logged into the correct AD account that has access to this resource
            _configurationBuilder.AddAzureAppConfiguration(options => { options.Connect(new Uri(_configuration["AppConfigurationConnection"]), new DefaultAzureCredential()); });

            // Add KeyVault - auth is default azure credential, therefore your VS must be logged into the correct AD account that has access to this resource
            _configurationBuilder.AddAzureKeyVault(new Uri(_configuration["KeyVault"]), new DefaultAzureCredential());
        }
    }
}