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
    public class ExtractDataLakeFO : ExtractDataLakeSequential
    {
        public ExtractDataLakeFO(ILogger<ExtractDataLakeFO> logger, IConfiguration configuration, DictionaryService dictionaryService, StopwatchService stopWatchService, FandODataBaseAuthService fandODataBaseAuthService) : base(logger, configuration, stopWatchService, dictionaryService)
        {
            DataLakeConnectionString = fandODataBaseAuthService.GetConnectionString();
        }
    }
}