using CCBA.Integrations.Base.Models;
using Microsoft.Data.SqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    public static class SqlCommandExtensions
    {
        [Obsolete("Please use dependency injection version of this method.")]
        public static SqlParameter[] AddArrayParameters<T>(this SqlCommand sqlCommand, string paramNameRoot, IEnumerable<T> values, SqlDbType? dbType = null, int? size = null)
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
                if (dbType.HasValue)
                    sqlParameter.SqlDbType = dbType.Value;
                if (size.HasValue)
                    sqlParameter.Size = size.Value;
                sqlCommand.Parameters.Add(sqlParameter);
                parameters.Add(sqlParameter);
            }

            sqlCommand.CommandText = sqlCommand.CommandText.Replace("{" + paramNameRoot + "}", string.Join(",", parameterNames));

            return parameters.ToArray();
        }

        [Obsolete("Please use dependency injection version of this method.")]
        public static async Task<SqlDataReader> ExecuteReaderDataLakeAsync(this SqlCommand sqlCommand, CancellationToken cancellationToken = default)
        {
            var retryPolicy = Policy.Handle<DataLakeException>().WaitAndRetryAsync(10,
            attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), (_, _) => Task.CompletedTask);

            return await retryPolicy.ExecuteAsync(async (context, token) =>
            {
                try
                {
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
                    */

                    // If the error is related to external file handling in data lake, trigger a retry by throwing SqlPollyException
                    if (e.ToString().Contains("Error handling external file")) throw new DataLakeException(e.Message, e.InnerException);

                    throw;
                }
            }, new Context(), cancellationToken);
        }

        public static string GetSqlQuery(this SqlCommand sqlCommand)
        {
            var query = sqlCommand.CommandText;
            var parameters = sqlCommand.Parameters.Cast<SqlParameter>().AsQueryable();
            return Enumerable.Aggregate(parameters.OrderByDescending(x => x.ParameterName), query, (current, parameter) => current.Replace(parameter.ParameterName, parameter.GetValue()));
        }

        public static string GetValue(this SqlParameter sqlParameter)
        {
            string value;

            switch (sqlParameter.SqlDbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.Time:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    value = "'" + sqlParameter.Value.ToString()?.Replace("'", "''") + "'";
                    break;

                case SqlDbType.Bit:
                    value = (sqlParameter.Value.ToBooleanOrDefault(false)) ? "1" : "0";
                    break;

                case SqlDbType.BigInt:
                case SqlDbType.Binary:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Image:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.Real:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                case SqlDbType.VarBinary:
                case SqlDbType.Variant:
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                default:
                    value = sqlParameter.Value.ToString()?.Replace("'", "''");
                    break;
            }

            return value;
        }
    }
}