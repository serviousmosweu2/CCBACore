using Newtonsoft.Json;

namespace CCBA.Integrations.DMF.Import.Models
{
    public class AzureWritableURL
    {
        [JsonProperty("uniqueFileName")] public string UniqueFileName { get; set; }
    }
}