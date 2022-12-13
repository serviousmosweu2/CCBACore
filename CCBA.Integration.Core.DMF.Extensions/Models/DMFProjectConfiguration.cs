using CCBA.Integration.Core.DMF.Extensions.Models;

namespace CCBA.Infinity
{
    public class DMFProjectConfiguration
    {
        public string Integration { get; set; }
        public string ThirdParty { get; set; }
        public ExportConfiguration Export { get; set; }
        public ImportConfiguration Import { get; set; }
        public D365FOApplicationSettings D365FOSettings { get; set; }
        public int OperationType { get; internal set; }
    }
}