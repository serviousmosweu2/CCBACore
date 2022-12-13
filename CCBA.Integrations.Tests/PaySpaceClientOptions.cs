using CCBA.Integrations.Base.ServiceModels.ReusableServices;

namespace CCBA.Integrations.Tests
{
    public class PaySpaceClientOptions : ODataServiceOptions
    {
        public PaySpaceClientOptions(string clientName, string apiBase) : base(clientName, apiBase)
        {
        }
    }
}