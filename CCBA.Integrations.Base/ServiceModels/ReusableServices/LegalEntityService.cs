using System;
using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCBA.Integrations.Base.Helpers;
using Microsoft.Data.SqlClient;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Robson Beans
    /// Dependencies: <see cref="DictionaryService"/>
    /// </summary>
    public class LegalEntityService : BaseLogger
    {
        private readonly DictionaryService _dictionaryService;
        private readonly AisDataBaseAuthService _aisDataBaseAuthService;

        public LegalEntityService(ILogger<LegalEntityService> logger, IConfiguration configuration, DictionaryService dictionaryService, AisDataBaseAuthService aisDataBaseAuthService) : base(logger, configuration)
        {
            _dictionaryService = dictionaryService;
            _aisDataBaseAuthService = aisDataBaseAuthService;
        }

        /// <summary>
        /// Gets the legal entities for the specified integration
        /// </summary>
        /// <param name="integration">Required</param>
        /// <param name="connectionString">Optional</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<List<string>> GetLegalEntities(string integration, string connectionString = null)
        {
            if (integration == null) throw new ArgumentNullException(nameof(integration));
            
            connectionString ??= _aisDataBaseAuthService.GetConnectionString();
            
            var sql = $"SELECT * FROM vw_Integration_LegalEntities WHERE INTEGRATION_NAME = '{integration}'";
            var data = await _dictionaryService.GetPopulatedDictionary(sql, connectionString);
            var legalEntities = data.Select(s => s["legal_entity"].ToString()).ToList();

            return legalEntities;
        }
    }
}