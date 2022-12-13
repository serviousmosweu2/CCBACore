using Newtonsoft.Json;

namespace CCBA.Integrations.DMF.Shared.Models
{
    public class Summary
    {
        [JsonProperty("executionId")] public string ExecutionId { get; set; }
    }
}