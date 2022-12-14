using CCBA.Integrations.Base.Interfaces;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CCBA.Integrations.Tests
{
    public class PaySpaceClient : ODataService
    {
        public PaySpaceClient(ILogger<PaySpaceClient> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ODataServiceOptions oDataServiceOptions = null, IOAuthService oAuthService = null) : base(logger, configuration, httpClientFactory, oDataServiceOptions, oAuthService)
        {
            RetryPolicyHandlers.Add(PolicyHandleRetryAfterEnsureSuccess);
        }
    }
}