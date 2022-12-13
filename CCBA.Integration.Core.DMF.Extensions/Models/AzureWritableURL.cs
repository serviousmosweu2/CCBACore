using Newtonsoft.Json;

namespace CCBA.Integration.Core.DMF.Extensions.Models
{
    public class AzureWritableURL
    {
        [JsonProperty("uniqueFileName")]
        public string UniqueFileName { get; set; }
    }
}