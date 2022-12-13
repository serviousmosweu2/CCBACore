using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public static class SqlConnectionExtensions
    {
        public static async Task ReadData(string connectionString, string sql, Action<SqlDataReader> action)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                action.Invoke(reader);
            }
        }

        /// <summary>
        /// Not Tested yet!
        /// </summary>
        /// <param name="input"></param>
        /// <param name="conn"></param>
        /// <param name="destinationTableName"></param>
        private static void BulkInsert(this List<Dictionary<string, object>> input, string conn, string destinationTableName)
        {
            var tbl = new DataTable();

            foreach (var rows in input)
            {
                var dr = tbl.NewRow();
                foreach (var keyValuePair in rows)
                {
                    dr[keyValuePair.Key] = keyValuePair.Value;
                    tbl.Rows.Add(dr);
                }
            }

            var con = new SqlConnection(conn);
            var objbulk = new SqlBulkCopy(con);
            objbulk.DestinationTableName = destinationTableName;
            foreach (var keyValuePair in input.First())
            {
                objbulk.ColumnMappings.Add(keyValuePair.Key, keyValuePair.Key);
            }

            con.Open();
            objbulk.WriteToServer(tbl);
            con.Close();
        }
    }
}