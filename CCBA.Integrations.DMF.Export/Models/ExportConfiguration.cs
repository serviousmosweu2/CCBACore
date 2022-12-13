using CCBA.Integrations.DMF.Shared.Models;

namespace CCBA.Integrations.DMF.Export.Models
{
    public class ExportConfiguration : Configuration
    {
        public string packageName { get; set; }
        public bool? reExecute { get; set; }
    }
}