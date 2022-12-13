using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// </summary>
    public class MySqlService : BaseLogger
    {
        public MySqlService(ILogger<MySqlService> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public enum DBStatuses
        {
            Failed = 1,
            Succeeded = 2
        }

        public async Task<DBOpStatus> Update(string connectionString, string storedProcedure, Action<MySqlCommand> operation, TimeSpan? timeSpan = null)
        {
            var opStatus = new DBOpStatus { status = (int)DBStatuses.Failed };

            await Policy.Handle<MySqlPollyException>().WaitAndRetryAsync(15, attempt =>
            {
                var fromSeconds = timeSpan ?? TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt));
                return fromSeconds;
            },
            async (exception, timeSpan, retryCount, context) => { await ExceptionHandler(exception, timeSpan, retryCount, context); }).ExecuteAsync(async () =>
            {
                try
                {
                    await using var mySqlConnection = new MySqlConnection(connectionString);
                    await mySqlConnection.OpenAsync();

                    var mySqlCommand = new MySqlCommand(storedProcedure, mySqlConnection) { CommandType = CommandType.StoredProcedure };
                    operation.Invoke(mySqlCommand);

                    mySqlCommand.ExecuteNonQuery();
                    opStatus.status = (int)DBStatuses.Succeeded;
                }
                catch (Exception e)
                {
                    opStatus.exception = e.GetAllMessages();

                    var message = e.ToString().ToLower();
                    if (message.Contains("timeout") || message.Contains("unable to connect to any of the specified mysql hosts"))
                    {
                        throw new MySqlPollyException(e.GetAllMessages());
                    }

                    throw;
                }
            });

            return opStatus;
        }

        protected virtual async Task ExceptionHandler(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            // Only logged as a warning because the operation will be retried.
            LogException(exception,
                logLevel: LogLevel.Warning,
                properties: new Dictionary<string, string>
                    {
                        { "TimeSpan", timeSpan.ToString() },
                        { "RetryCount", retryCount.ToString() }
                    }
            );
        }

        public class DBOpStatus
        {
            public string exception { get; set; }
            public string spName { get; set; }
            public int status { get; set; }
        }

        public class MySqlPollyException : Exception
        {
            public MySqlPollyException(string message) : base(message)
            {
            }
        }
    }
}