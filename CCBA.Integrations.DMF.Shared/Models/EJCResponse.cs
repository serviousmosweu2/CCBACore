using Newtonsoft.Json;

namespace CCBA.Integrations.DMF.Shared.Models
{
    public class EJCResponse
    {
        [JsonProperty("maintainanceId")]
        public string MaintainanceId { get; set; }
    }
}