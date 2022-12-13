using Newtonsoft.Json;
using System.Net.Http;

namespace CCBA.Integrations.Base.Helpers
{
    public static class HttpExtensions
    {
        public static bool IsJson(this HttpResponseMessage httpResponseMessage)
        {
            try
            {
                var content = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var _ = JsonConvert.DeserializeObject(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}