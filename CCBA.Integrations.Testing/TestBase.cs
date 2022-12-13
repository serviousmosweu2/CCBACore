using CCBA.Integrations.Testing.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NCrontab;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ConditionIsAlwaysTrueOrFalse
//#pragma warning disable CS0162

namespace CCBA.Integrations.Testing
{
    public abstract class ATestBase
    {
        protected IConfiguration _configuration;
        protected ConfigurationBuilder _configurationBuilder = new();
        protected ServiceCollection _serviceCollection = new();
        protected ServiceProvider _serviceProvider;
        protected TimerInfo _timerInfo;

        protected virtual void Build()
        {
            _configuration = _configurationBuilder.Build();

            _serviceCollection.AddSingleton(_configuration);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }

        protected abstract void Setup();
    }

    public abstract class TestBase : ATestBase
    {
        protected override void Setup()
        {
            _timerInfo = new TimerInfo(new CronSchedule(CrontabSchedule.Parse("*/10 * * * * *", new CrontabSchedule.ParseOptions { IncludingSeconds = true })), new ScheduleStatus());

            // Add your own keys to config for testing
            _configurationBuilder.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Ump:GZipEnabled", "true") });

            // Add external config files
            _configurationBuilder.AddJsonFile("local.settings.json", false, true);

            // Add services
            _serviceCollection.AddLogging(builder => { builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()); });
            _serviceCollection.AddHttpClient();
        }
    }
}