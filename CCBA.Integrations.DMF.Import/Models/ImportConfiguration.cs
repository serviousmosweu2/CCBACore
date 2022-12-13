using CCBA.Integrations.DMF.Shared.Models;

namespace CCBA.Integrations.DMF.Import.Models
{
    public class ImportConfiguration : Configuration
    {
        public bool execute { get; set; }
        public bool overwrite { get; set; }
        public string packageUrl { get; set; }
    }
}