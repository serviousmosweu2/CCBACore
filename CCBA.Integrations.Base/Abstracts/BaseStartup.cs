using Azure.Identity;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace CCBA.Integrations.Base.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Add a level of abstraction to add models and services.
    /// </summary>
    public abstract class BaseStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddODataService();

            builder.Services.AddTransient<AisDataBaseAuthService>();
            builder.Services.AddTransient<LegalEntityService>();
            builder.Services.AddTransient<DataTransformationAlternative>();

            builder.Services.AddTransient<DictionaryService>();
            builder.Services.AddTransient<ExtractDataLakeFO>();

            builder.Services.AddTransient<MySqlService>();
            builder.Services.AddTransient<SqlCommandService>();
            builder.Services.AddTransient<SqlDataReaderService>();

            builder.Services.AddTransient<StopwatchService>();

            builder.Services.AddTransient<CEDataBaseAuthService>();
            builder.Services.AddTransient<FandODataBaseAuthService>();

            ConfigureApp(builder);
        }

        public abstract void ConfigureApp(IFunctionsHostBuilder builder);

        /// <summary>
        /// Developer: Johan Nieuwenhuis, Dattatray Mharanur
        /// Initialize environment variables.
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var assembly = GetType().Assembly.GetName().Name;
            if (assembly == null || assembly.StartsWith("CCBA.Integrations")) assembly = Assembly.GetExecutingAssembly().GetName().Name;

            var build = builder.ConfigurationBuilder.AddJsonFile("local.settings.json", true, true);

            var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());

            var appConfig = Environment.GetEnvironmentVariable("AppConfigurationConnection");
            var keyVault = Environment.GetEnvironmentVariable("KeyVault");

            if (!string.IsNullOrWhiteSpace(appConfig))
            {
                build.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfig), defaultAzureCredential)
                        .ConfigureKeyVault(vaultOptions => vaultOptions.SetCredential(defaultAzureCredential))
                        .ConfigureRefresh(refreshOptions =>
                        {
                            refreshOptions.Register(KeyFilter.Any, true);
                            refreshOptions.SetCacheExpiration(TimeSpan.FromMinutes(1));
                        });
                    options.TrimKeyPrefix($"{assembly}:");
                });
            }

            if (!string.IsNullOrWhiteSpace(keyVault))
            {
                build.AddAzureKeyVault(new Uri(keyVault), defaultAzureCredential, new KeyVaultSecretManagerWithAssemblyTrim(assembly));
            }

            build = build.AddEnvironmentVariables();

            build.Build();
        }
    }
}