using Newtonsoft.Json;

namespace CCBA.Infinity
{
    public class EJCResponse
    {
        [JsonProperty("maintainanceId")]
        public string MaintainanceId { get; set; }
    }
}