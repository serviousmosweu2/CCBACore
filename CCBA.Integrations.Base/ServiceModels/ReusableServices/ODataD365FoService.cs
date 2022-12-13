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
    public class ODataD365FoService : ODataService
    {
        public ODataD365FoService(ILogger<ODataD365FoService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ODataServiceOptions oDataServiceOptions, IOAuthService oAuthService) : base(logger, configuration, httpClientFactory, oDataServiceOptions, oAuthService)
        {
        }
    }
}