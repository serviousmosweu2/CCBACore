using Newtonsoft.Json;

namespace CCBA.Infinity
{
    public class ImportSummary
    {
        [JsonProperty("executionId")]
        public string ExecutionId { get; set; }
    }
}