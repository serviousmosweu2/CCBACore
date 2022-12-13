using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class SqlDataReaderService : BaseLogger
    {
        public SqlDataReaderService(ILogger<BaseLogger> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public Dictionary<string, object> MapDbEntity(SqlDataReader sqlDataReader)
        {
            var dictionary = new Dictionary<string, object>();
            for (var i = 0; i < sqlDataReader.FieldCount; i++)
                dictionary[sqlDataReader.GetName(i).ToLowerClean()] = sqlDataReader.IsDBNull(i) ? null : sqlDataReader.GetValue(i);

            return dictionary;
        }

        public async Task ReadData(string connectionString, string sql, Action<SqlDataReader> action)
        {
            await using var connection = new SqlConnection(connectionString);
            var provider = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new SqlRetryLogicOption
            {
                NumberOfTries = 5,
                MaxTimeInterval = TimeSpan.FromSeconds(20),
                DeltaTime = TimeSpan.FromSeconds(1)
            });
            connection.RetryLogicProvider = provider;
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            var count = 0;
            while (await reader.ReadAsync())
            {
                action.Invoke(reader);
                count++;
            }
            LogInformation($@"{nameof(ReadData)} Complete.", properties: new Dictionary<string, string> { { "Number of Records", $@"{count}" } });
        }

        public async Task ReadData(string connectionString, SqlParameter[] sqlParameters, Action<SqlDataReader> action, string storedProc)
        {
            await using var connection = new SqlConnection(connectionString);
            var provider = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new SqlRetryLogicOption
            {
                NumberOfTries = 5,
                MaxTimeInterval = TimeSpan.FromSeconds(20),
                DeltaTime = TimeSpan.FromSeconds(1)
            });
            connection.RetryLogicProvider = provider;
            await connection.OpenAsync();
            var sqlCommand = new SqlCommand(storedProc, connection) { CommandType = CommandType.StoredProcedure };
            sqlCommand.Parameters.AddRange(sqlParameters);
            await using var reader = await sqlCommand.ExecuteReaderAsync();
            var count = 0;
            while (await reader.ReadAsync())
            {
                action.Invoke(reader);
                count++;
            }
            LogInformation($@"{nameof(ReadData)} Complete.", properties: new Dictionary<string, string> { { "Number of Records", $@"{count}" } });
        }
    }
}