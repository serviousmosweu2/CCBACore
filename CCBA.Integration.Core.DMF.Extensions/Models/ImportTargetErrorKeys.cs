using Newtonsoft.Json;

namespace CCBA.Infinity
{

    public class ImportTargetErrorKeys
    {
        [JsonProperty("executionId")]
        public string ExecutionId { get; set; }
        [JsonProperty("entityName")]
        public string EntityName { get; set; }
    }
}