using Azure;
using Azure.Data.Tables;
using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    public class DeltaStateService : BaseLogger
    {
        private readonly TableClient _tableClient;

        public DeltaStateService(ILogger<DeltaStateService> logger, IConfiguration configuration) : base(logger, configuration)
        {
            _tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "DeltaState");
            _tableClient.CreateIfNotExistsAsync();
        }

        public async Task<string> GetDeltaAsync(string tableName, string defaultValue = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _tableClient.GetEntityAsync<DeltaStateEntity>("DeltaState", tableName, cancellationToken: cancellationToken);

                LogInformation($"{nameof(GetDeltaAsync)} executed", LogLevel.Trace, properties: new Dictionary<string, string>
                {
                    { "TableName", tableName },
                    { "Delta", entity?.Value?.Delta ?? defaultValue }
                });

                return entity?.Value?.Delta ?? defaultValue;
            }
            catch
            {
                // ignore for now
            }

            return defaultValue;
        }

        public async Task SetDeltaAsync(string tableName, string delta, CancellationToken cancellationToken = default)
        {
            var entity = new DeltaStateEntity
            {
                RowKey = tableName,
                Delta = delta
            };

            await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);

            LogInformation($"{nameof(SetDeltaAsync)} executed", LogLevel.Trace, properties: new Dictionary<string, string>
            {
                { "TableName", tableName },
                { "Delta", delta }
            });
        }

        private class DeltaStateEntity : ITableEntity
        {
            public string Delta { get; set; }
            public ETag ETag { get; set; }
            public string PartitionKey { get; set; } = "DeltaState";
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
        }
    }
}