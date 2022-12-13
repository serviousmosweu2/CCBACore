using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="SqlDataReaderService"/>
    /// </summary>
    public class DictionaryService : BaseLogger
    {
        private readonly SqlDataReaderService _sqlDataReaderService;

        public DictionaryService(ILogger<DictionaryService> logger, IConfiguration configuration, SqlDataReaderService sqlDataReaderService) : base(logger, configuration)
        {
            _sqlDataReaderService = sqlDataReaderService;
        }

        public async Task<List<Dictionary<string, object>>> GetPopulatedDictionary(string sql, string conn)
        {
            var data = new List<Dictionary<string, object>>();

            await _sqlDataReaderService.ReadData(conn, sql, dataReader => { data.Add(_sqlDataReaderService.MapDbEntity(dataReader)); });

            return data;
        }
    }
}