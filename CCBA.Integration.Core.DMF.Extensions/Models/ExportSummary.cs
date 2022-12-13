using Newtonsoft.Json;

namespace CCBA.Infinity
{
    public class ExportSummary
    {
        [JsonProperty("executionId")]
        public string ExecutionId { get; set; }
    }
}