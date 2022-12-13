using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="SqlDataReaderService"/>
    /// </summary>
    public class DataTransformationAlternative : BaseLogger
    {
        private static readonly object Locker = new object();
        private static volatile List<Dictionary<string, object>> _cache = new List<Dictionary<string, object>>();
        private readonly SqlDataReaderService _sqlDataReaderService;

        public DataTransformationAlternative(ILogger<DataTransformationAlternative> logger, IConfiguration configuration, SqlDataReaderService sqlDataReaderService) : base(logger, configuration)
        {
            _sqlDataReaderService = sqlDataReaderService;
        }

        private static DateTime LastUpdated { get; set; }

        public async Task<string> GetDataMappingAsync(string sourceSystem, string targetSystem, string integrationName, string fieldName, string sourceValue)
        {
            if ((DateTime.Now - LastUpdated).TotalMinutes > 5)
            {
                await RefreshCache(sourceSystem, targetSystem, integrationName);
                LastUpdated = DateTime.Now;
            }

            var data = _cache.Where(x => Equals(x["sourcesystem"], sourceSystem)
                                         && Equals(x["targetsystem"], targetSystem)
                                         && Equals(x["integration"], integrationName)
                                         && Equals(x["fieldname"], fieldName)
                                         && Equals(x["sourcevalue"], sourceValue)).ToList();
            if (data.Count == 0)
            {
                data = _cache.Where(x => Equals(x["sourcesystem"], sourceSystem)
                                         && Equals(x["targetsystem"], targetSystem)
                                         && Equals(x["integration"], integrationName)
                                         && Equals(x["fieldname"], fieldName)
                                         && x["sourcevalue"] == null
                                         && x["targetvalue"] == null).ToList();
            }

            Dictionary<string, object> dataMappingResponse = null;
            if (data.Count == 1)
            {
                dataMappingResponse = data.First();
            }

            if (dataMappingResponse != null && dataMappingResponse.ContainsKey("targetvalue"))
            {
                return dataMappingResponse["targetvalue"].ToString();
            }

            throw new ApplicationException($"Mapping not found for field {fieldName} and value {sourceValue} inside systems {sourceSystem} --> {targetSystem}");
        }

        private async Task<List<Dictionary<string, object>>> GetPopulatedDictionary(SqlParameter[] sqlParameters, string conn, string storedProc)
        {
            var data = new List<Dictionary<string, object>>();
            await _sqlDataReaderService.ReadData(conn, sqlParameters, dataReader => { data.Add(_sqlDataReaderService.MapDbEntity(dataReader)); }, storedProc);

            return data;
        }

        private async Task RefreshCache(string sourceSystem, string targetSystem, string integrationName)
        {
            LogInformation("Refreshing Cache Start");
            string conn;

            if (EnvironmentExtensions.IsDevelopment)
            {
                conn = $@"Server={Configuration["AISDataSource"]};Initial Catalog={Configuration["AISInitialCatalog"]};Persist Security Info=False;User ID={Configuration["ManagedIdentityId"]};Authentication=ActiveDirectoryInteractive;Connection Timeout=1680;";
            }
            else
            {
                conn = $@"Data Source={Configuration["AISDataSource"]};Initial Catalog={Configuration["AISInitialCatalog"]};User ID={Configuration["ManagedIdentityId"]};Authentication=ActiveDirectoryManagedIdentity;Command Timeout=1680";
            }
            await Task.Delay(1);
            lock (Locker)
            {
                _cache = GetPopulatedDictionary(new[]
                {
                    new SqlParameter("@sourceSystem", sourceSystem),
                    new SqlParameter("@targetSystem", targetSystem),
                    new SqlParameter("@integrationName", integrationName)
                }, conn, "dbo.GetMappingData").Result;
            }
            LogInformation("Refreshing Cache End");
        }
    }
}