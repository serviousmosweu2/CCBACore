using CCBA.Integrations.Base.ServiceModels.ReusableServices;

namespace CCBA.Integrations.Tests
{
    public class PaySpaceExtractionClientOptions : ODataServiceOptions
    {
        public PaySpaceExtractionClientOptions(string clientName, string apiBase) : base(clientName, apiBase)
        {
        }
    }
}