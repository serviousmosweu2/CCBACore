using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.DataTransformation.Common;
using CCBA.Integrations.DataTransformation.Interfaces;
using CCBA.Integrations.DataTransformation.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCBA.Integrations.DataTransformation.Services
{
    public class DataTransformationService : IDataTransformationService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;

        public DataTransformationService(IConfiguration configuration, IDistributedCache distributedCache)
        {
            _configuration = configuration;
            _distributedCache = distributedCache;
        }

        public async Task<DataMappingResponse> GetDataMappingAsync(string sourceSystem, string targetSystem, string integrationName, string fieldName, string sourceValue)
        {
            var dataMappingResponse = new DataMappingResponse();

            var cacheKey = $"{nameof(DataTransformationService)}_{integrationName}_{sourceSystem}_{targetSystem}";

            var cachedData = await _distributedCache.GetStringAsync(cacheKey);

            var dataMappingResponses = !string.IsNullOrEmpty(cachedData) ? JsonSerializer.Deserialize<List<DataMappingResponse>>(cachedData) : new List<DataMappingResponse>();
            if (dataMappingResponses == null || dataMappingResponses.Count == 0)
            {
                dataMappingResponses = new List<DataMappingResponse>();

                var connectionBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = _configuration["AISDataSource"],
                    UserID = _configuration["ManagedIdentityId"],
                    InitialCatalog = _configuration["AISInitialCatalog"],
                    Authentication = EnvironmentExtensions.IsDevelopment ? SqlAuthenticationMethod.ActiveDirectoryInteractive : SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                    CommandTimeout = 180,
                    PersistSecurityInfo = true
                };

                var retryLogicProvider = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new SqlRetryLogicOption
                {
                    NumberOfTries = 5,
                    MinTimeInterval = TimeSpan.FromSeconds(1),
                    MaxTimeInterval = TimeSpan.FromSeconds(3),
                    DeltaTime = TimeSpan.FromSeconds(1)
                });

                await using var sqlConnection = new SqlConnection(connectionBuilder.ConnectionString);
                sqlConnection.RetryLogicProvider = retryLogicProvider;
                await sqlConnection.OpenAsync();

                var sqlCommand = new SqlCommand("dbo.GetMappingData", sqlConnection) { CommandType = CommandType.StoredProcedure, RetryLogicProvider = retryLogicProvider };
                sqlCommand.Parameters.Add(new SqlParameter("@sourceSystem", sourceSystem));
                sqlCommand.Parameters.Add(new SqlParameter("@targetSystem", targetSystem));
                sqlCommand.Parameters.Add(new SqlParameter("@integrationName", integrationName));
                await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

                while (await sqlDataReader.ReadAsync()) dataMappingResponses.Add(sqlDataReader.MapDbEntity<DataMappingResponse>());

                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dataMappingResponses), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_configuration.GetValue("DataMapping:CacheDurationMinutes", 1))
                });
            }

            var data = dataMappingResponses.Where(x => x.FieldName == fieldName && x.SourceValue == sourceValue).ToList();
            if (data.Count == 0) data = dataMappingResponses.Where(x => x.FieldName == fieldName && x.SourceValue == null && x.TargetValue == null).ToList();
            if (data.Count == 1) dataMappingResponse = data.First();

            dataMappingResponse.Status = data.Count > 1 ? TransformationStatus.OneToMany : data.Count == 1 ? TransformationStatus.Found : TransformationStatus.NotFound;

            return dataMappingResponse;
        }
    }
}