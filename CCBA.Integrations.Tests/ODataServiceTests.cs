using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using CCBA.Integrations.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CCBA.Integrations.Tests
{
    [TestClass]
    public class ODataServiceTests
    {
        private IConfigurationRoot _configuration;
        private ILogger<ODataServiceTests> _logger;
        private ILoggerFactory _loggerFactory;

        [TestMethod]
        public async Task ODataD365CeServiceCreation()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()));
            services.AddSingleton<IConfiguration>(_configuration);

            var authOptions = services.GetODataD365CeServiceOptions();
            services.AddODataD365CeService(Assembly.GetCallingAssembly(), authOptions);

            var serviceProvider = services.BuildServiceProvider();

            var odataService = serviceProvider.GetService<ODataD365CeService>();

            Assert.IsNotNull(odataService);

            _logger.LogInformation($"Token={!string.IsNullOrEmpty(odataService.HttpClient.DefaultRequestHeaders.Authorization.Parameter)}");

            /*odataService.RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3,
            attempt => TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt)), async (exception, calculatedWaitDuration) => { /*await ExceptionLogic(exception);#1# });*/

            //await odataService.AuthenticateAsync();
        }

        [TestMethod]
        public async Task ODataD365FoServiceCreation()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()));
            services.AddSingleton<IConfiguration>(_configuration);

            services.AddODataD365FoService(Assembly.GetCallingAssembly(), services.GetODataD365FoServiceOptions());

            var serviceProvider = services.BuildServiceProvider();

            var odataService = serviceProvider.GetService<ODataD365FoService>();

            Assert.IsNotNull(odataService);

            _logger.LogInformation($"Token={!string.IsNullOrEmpty(odataService.HttpClient.DefaultRequestHeaders.Authorization.Parameter)}");

            /*odataService.RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3,
            attempt => TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt)), async (exception, calculatedWaitDuration) => { /*await ExceptionLogic(exception);#1# });*/

            /*foreach (var i in Enumerable.Range(1, 3))
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await odataService.GetAsync($"https://devopssec-functions.azurewebsites.net/api/HttpTest429");
                _logger.LogInformation($"Id={i} Duration={stopwatch.Elapsed} StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }*/
        }

        [TestMethod]
        public async Task ODataServiceWithoutRetry()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()));
            services.AddSingleton<IConfiguration>(_configuration);

            services.AddODataService(new ODataServiceOptions("Test", null)
            {
                UseAuthorization = false,
                UseRetryPolicy = false
            });

            var serviceProvider = services.BuildServiceProvider();

            var odataService = serviceProvider.GetService<ODataService>();

            Assert.IsNotNull(odataService);

            foreach (var i in Enumerable.Range(1, 5))
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await odataService.GetAsync("https://devopssec-functions.azurewebsites.net/api/HttpTest429");
                _logger.LogInformation($"Id={i} Duration={stopwatch.Elapsed} StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }
        }

        [TestMethod]
        public async Task ODataServiceWithRetry()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()));
            services.AddSingleton<IConfiguration>(_configuration);

            services.AddODataService(new ODataServiceOptions("Test", null)
            {
                UseAuthorization = false,
                UseRetryPolicy = true,
                RetryIsExponential = false,
                RetryDelay = 100,
                RetryCount = 15
            });

            var serviceProvider = services.BuildServiceProvider();

            var odataService = serviceProvider.GetService<ODataService>();

            Assert.IsNotNull(odataService);

            foreach (var i in Enumerable.Range(1, 5).AsParallel())
            {
                var stopwatch = Stopwatch.StartNew();
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://devopssec-functions.azurewebsites.net/api/HttpTest429");
                var response = await odataService.SendAsync(httpRequestMessage);
                _logger.LogInformation($"Id={i} Duration={stopwatch.Elapsed} StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }
        }

        [TestMethod]
        public void PaySpaceClientWithRetry()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()));
            services.AddSingleton<IConfiguration>(_configuration);
            services.AddHttpClient();

            /*services.AddTransient(provider => new PaySpaceClientOptions("PaySpace", null) { RetryCount = 10 });
            services.AddTransient<PaySpaceClient>();*/

            services.AddPaySpaceClient();
            services.AddPaySpaceExtractionClient();

            var serviceProvider = services.BuildServiceProvider();

            var paySpaceClient = serviceProvider.GetService<PaySpaceClient>();

            paySpaceClient.Should().NotBeNull();

            Parallel.ForEach(Enumerable.Range(1, 10), i =>
            {
                Task.Run(async () =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    var response = await paySpaceClient.GetAsync("https://devopssec-functions.azurewebsites.net/api/HttpTest400");
                    _logger.LogInformation($"Id={i} Duration={stopwatch.Elapsed} StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                }).Wait();
            });
        }

        [TestInitialize]
        public void Setup()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>());

            _configuration = configurationBuilder.Build();

            _loggerFactory = LoggerFactory.Create(builder => { builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()); });

            _logger = _loggerFactory.CreateLogger<ODataServiceTests>();
        }
    }
}