using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="DictionaryService"/>, <see cref="FandODataBaseAuthService"/>
    /// </summary>
    public class ExtractDataLakeCE : ExtractDataLakeSequential
    {
        public ExtractDataLakeCE(ILogger<ExtractDataLakeCE> logger, IConfiguration configuration, DictionaryService dictionaryService, StopwatchService stopWatchService, CEDataBaseAuthService ceDataBaseAuthService) : base(logger, configuration, stopWatchService, dictionaryService)
        {
            DataLakeConnectionString = ceDataBaseAuthService.GetConnectionString();
        }
    }
}