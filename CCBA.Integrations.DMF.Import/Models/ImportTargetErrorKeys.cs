using CCBA.Integrations.DMF.Shared.Models;
using Newtonsoft.Json;

namespace CCBA.Integrations.DMF.Import.Models
{
    public class ImportTargetErrorKeys : Summary
    {
        [JsonProperty("entityName")] public string EntityName { get; set; }
    }
}