using CCBA.Integrations.Base.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Konrad Steynberg
    /// Dependencies: <see cref="ODataServiceOptions"/>, <see cref="IOAuthService"/>, <see cref="IHttpClientFactory"/>
    /// </summary>
    public class ODataD365CeService : ODataService
    {
        public ODataD365CeService(ILogger<ODataD365CeService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ODataServiceOptions oDataServiceOptions, IOAuthService oAuthService) : base(logger, configuration, httpClientFactory, oDataServiceOptions, oAuthService)
        {
        }
    }
}