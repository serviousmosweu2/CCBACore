using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Konrad Steynberg
    /// </summary>
    public class SqlCommandService : BaseLogger
    {
        private readonly bool _loggingSqlQueriesEnabled;

        public SqlCommandService(ILogger<SqlCommandService> logger, IConfiguration configuration) : base(logger, configuration)
        {
            _loggingSqlQueriesEnabled = configuration.GetValue("Logging:Sql:Queries:Enabled", false);
        }

        /// <summary>
        /// This will add an array of parameters to a SqlCommand. This is used for an IN statement.
        /// Use the returned value for the IN part of your SQL call. (i.e. SELECT * FROM table WHERE field IN ({paramNameRoot}))
        /// </summary>
        /// <param name="sqlCommand">The SqlCommand object to add parameters to.</param>
        /// <param name="paramNameRoot">What the parameter should be named followed by a unique value for each value. This value surrounded by {} in the CommandText will be replaced.</param>
        /// <param name="values">The array of strings that need to be added as parameters.</param>
        /// <param name="dbType">One of the System.Data.SqlDbType values. If null, determines type based on T.</param>
        /// <param name="size">The maximum size, in bytes, of the data within the column. The default value is inferred from the parameter value.</param>
        public SqlParameter[] AddArrayParameters<T>(SqlCommand sqlCommand, string paramNameRoot, IEnumerable<T> values, SqlDbType? dbType = null, int? size = null)
        {
            /* An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually.
             * Each item in the array will end up being it's own SqlParameter so the return value for this must be used as part of the
             * IN statement in the CommandText.
             */
            var parameters = new List<SqlParameter>();
            var parameterNames = new List<string>();
            var paramNumber = 1;
            foreach (var value in values)
            {
                var paramName = $"@{paramNameRoot}{paramNumber++}";
                parameterNames.Add(paramName);
                var sqlParameter = new SqlParameter(paramName, value);
                if (dbType.HasValue) sqlParameter.SqlDbType = dbType.Value;
                if (size.HasValue) sqlParameter.Size = size.Value;
                sqlCommand.Parameters.Add(sqlParameter);
                parameters.Add(sqlParameter);
            }

            sqlCommand.CommandText = sqlCommand.CommandText.Replace("{" + paramNameRoot + "}", string.Join(",", parameterNames));

            return parameters.ToArray();
        }

        public virtual async Task ExceptionHandler(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            var totalDuration = default(TimeSpan);

            var properties = new Dictionary<string, string>
            {
                { "TimeSpan", timeSpan.ToString() },
                { "RetryCount", retryCount.ToString() },
                { "ExceptionType", exception.GetType().ToString() }
            };

            // Copy context keys to properties
            foreach (var key in context.Keys) properties.TryAdd(key, context[key].ToString());

            // Copy exception data keys to properties
            foreach (DictionaryEntry entry in exception.Data) properties.TryAdd(entry.Key.ToString(), entry.Value?.ToString());

            // Calculate total duration
            if (context.ContainsKey("StartedAt"))
            {
                context.TryGetValue("StartedAt", out var startedAtObject);
                DateTime.TryParse(startedAtObject?.ToString(), out var startedAt);
                totalDuration = DateTime.UtcNow - startedAt;
            }

            if (totalDuration != default) properties.Add("TotalDuration", totalDuration.ToString());

            LogInformation(exception.Message, LogLevel.Warning, properties: properties);
        }

        /// <summary>
        /// Equivalent of ExecuteReaderAsync for Azure Data Lake
        /// This will retry on exceptions related to handling of external files in Data Lake
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="minRetryMilliseconds"></param>
        /// <param name="retryAttempts"></param>
        /// <param name="trackedProperties"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="DataLakeException"></exception>
        public async Task<SqlDataReader> ExecuteReaderDataLakeAsync(SqlCommand sqlCommand, CancellationToken cancellationToken = default, int minRetryMilliseconds = 5000, int retryAttempts = 24,
            Dictionary<string, string> trackedProperties = null, [CallerMemberName] string method = "")
        {
            var startedAt = DateTime.UtcNow;
            var contextData = new Context { { "StartedAt", startedAt } };

            trackedProperties ??= new Dictionary<string, string>();
            trackedProperties.TryAdd("StartedAt", startedAt.ToString("O"));
            trackedProperties.TryAdd("CallerMemberName", method);

            if (_loggingSqlQueriesEnabled)
            {
                contextData.Add("SqlQuery", sqlCommand.GetSqlQuery());
                trackedProperties.Add("SqlQuery", sqlCommand.GetSqlQuery());
            }

            LogInformation($"Executing {nameof(ExecuteReaderDataLakeAsync)}", LogLevel.Information, properties: trackedProperties);

            var dataLakePollyTestingRetryCount = 0;
            var dataLakePollyTestingEnabled = Configuration.GetValue("DataLake:PollyTesting:Enabled", false);
            var dataLakeRetryTraps = Configuration.GetValue("DataLakeRetryTraps", "Error handling external file|timeout|DataLakePollyTest");

            var retryPolicy = Policy.Handle<DataLakeException>().WaitAndRetryAsync(retryAttempts, attempt => TimeSpan.FromMilliseconds(minRetryMilliseconds),
            async (exception, timeSpan, retryCount, context) => await ExceptionHandler(exception, timeSpan, retryCount, context));

            var dataReader = await retryPolicy.ExecuteAsync(async (context, token) =>
            {
                try
                {
                    LogInformation("Executing policy", LogLevel.Information, properties: trackedProperties);

                    if (dataLakePollyTestingEnabled && dataLakePollyTestingRetryCount < retryAttempts)
                    {
                        var dataLakePollyTestingExceptionMessage = Configuration.GetValue("DataLake:PollyTesting:ExceptionMessage", "Error handling external file: Data Lake Polly Testing Enabled");

                        dataLakePollyTestingRetryCount++;
                        throw new Exception(dataLakePollyTestingExceptionMessage);
                    }

                    var sqlDataReader = await sqlCommand.ExecuteReaderAsync(token);
                    return sqlDataReader;
                }
                catch (Exception e)
                {
                    /*
                    Error handling external file: 'Unexpected end-of-input within record at [byte: 4289980]. '. File/External table name: 'Tables.LOGISTICSLOCATION'.
                    Statement ID: {6C46D43B-14B5-4C88-9CF8-3D34FFF91FD1} | Query hash: 0xC4D5A2680338F3ED | Distributed request ID: {39E7ADA7-9B43-43B8-B52B-A09D04E0A429}. Total size of data scanned is 68 megabytes, total size of data moved is 5 megabytes, total size of data written is 0 megabytes.

                    Error handling external file: 'Async IO failed. ERROR = 0x4C7.'. File/External table name: 'Tables.LOGISTICSELECTRONICADDRESS'.
                    Statement ID: {AD9A9C38-E612-41C4-8E0D-1E878F1D2561} | Query hash: 0x973DD882316F2E47 | Distributed request ID: {A4F254B4-52CB-45CC-AE20-D38F036C378E}. Total size of data scanned is 14 megabytes, total size of data moved is 1 megabytes, total size of data written is 0 megabytes.

                    Error handling external file: 'waitIOCompletion error. HRESULT = 0x800704C7(offset = 0, bytes requested = 594917).'. File/External table name: 'Tables.ECORESCATEGORY'.
                    Statement ID: {3AAD8CFE-A565-4C45-B361-2D363E4F830A} | Query hash: 0x5F25AF59023B5EC | Distributed request ID: {FF553484-66FB-4736-9FCA-AD73B0A595FD}. Total size of data scanned is 244 megabytes, total size of data moved is 94 megabytes, total size of data written is 0 megabytes.

                    Error handling external file: 'Async IO failed to read requested number of bytes (offset = 44040192, expected = 631474, actual = 631472).'. File/External table name: 'Tables.INVENTTABLEMODULE'.
                    Statement ID: {CC357C41-C45F-4284-8F27-CBE8996D84B9} | Query hash: 0x5F25AF59023B5EC | Distributed request ID: {1EB8AF1B-178D-4E11-A59D-79759BA22CD9}. Total size of data scanned is 100 megabytes, total size of data moved is 50 megabytes, total size of data written is 0 megabytes.

                    Testing: Option 1

                    If the error is related to external file handling in data lake, trigger a retry by throwing DataLakeException

                    To test you can submit a malformed query for example SELECT * WHERE 1 = DataLakePollyTest
                    The above query will return an error as follows:

                        Msg 207, Level 16, State 1, Line 1
                        Invalid column name 'DataLakePollyTest'.
                            Msg 263, Level 16, State 1, Line 1
                        Must specify table to select from.

                    If we see the phrase DataLakePollyTest echoed back in the error we will apply polly retry logic.

                    Testing: Option 2

                    Set a configuration value of true for DataLake:PollyTesting:Enabled
                    This will trigger an exception on every call made by ExecuteReaderDataLakeAsync except for the last retry attempt

                    */

                    string[] dataLakeRetryTrapsArray = dataLakeRetryTraps.Split('|');
                    if (dataLakeRetryTrapsArray.Length > 0)                    {
                        LogInformation("DataLakeRetryTraps: " + dataLakeRetryTraps, LogLevel.Information, properties: trackedProperties);
                        foreach (string retryTrap in dataLakeRetryTrapsArray)                        {                            if (e.ToString().Contains(retryTrap))                            {                                var dataLakeException = new DataLakeException(e.Message, e.InnerException);                                foreach (var key in context.Keys) dataLakeException.Data.Add(key, context[key]);                                throw dataLakeException;                            }                        }
                    }

                    foreach (var key in context.Keys) e.Data.Add(key, context[key]);
                    throw;
                }
                finally
                {
                    LogInformation("Executed policy", LogLevel.Information, properties: trackedProperties);
                }
            }, contextData, cancellationToken);

            LogInformation($"Executed {nameof(ExecuteReaderDataLakeAsync)}", LogLevel.Information, properties: trackedProperties);

            return dataReader;
        }

        private static Dictionary<string, string> GetSqlParameters(IEnumerable parameters)
        {
            var dictionary = new Dictionary<string, string>();
            var enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var sqlParameter = (SqlParameter)enumerator.Current;
                if (sqlParameter != null) dictionary.TryAdd(sqlParameter.ParameterName, sqlParameter.GetValue());
            }

            return dictionary;
        }
    }
}