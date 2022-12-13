using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.DataTransformation;
using CCBA.Integrations.DataTransformation.Interfaces;
using CCBA.Integrations.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Tests
{
    [TestClass]
    public class DataTransformationServiceTests
    {
        private IConfiguration _configuration;
        private ILogger<BaseLogger> _logger;
        private ServiceProvider _serviceProvider;

        [TestInitialize]
        public void Setup()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("local.settings.json", false, true);
            _configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton(_configuration);
            services.AddLogging(builder => { builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()); });

            services.AddDataTransformationService();

            _serviceProvider = services.BuildServiceProvider();

            _logger = _serviceProvider.GetService<ILogger<BaseLogger>>();
            _logger.LogInformation("Setup complete");

            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        }

        [TestMethod]
        public async Task TestDataTransformationService()
        {
            var dataTransformationService = _serviceProvider.GetRequiredService<IDataTransformationService>();

            foreach (var i in Enumerable.Range(1, 100))
            {
                var mapping = await dataTransformationService.GetDataMappingAsync("aDSD", "D365F&O", "IDD-LEW-012 Send Sales Invoice", "VISITSTATUS", "1");
                mapping.TargetValue.Should().Be("PROCESSED");
                _logger.LogInformation(JsonConvert.SerializeObject(mapping));
            }
        }
    }
}