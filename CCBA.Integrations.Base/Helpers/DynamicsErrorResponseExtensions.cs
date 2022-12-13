using CCBA.Integrations.Base.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.Helpers
{
    public static class DynamicsErrorResponseExtensions
    {
        /// <summary>
        /// Returns the first most details error message from the http response message content
        /// If the content is not valid json, returns the raw content string
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<string> GetDynamicsErrorMessageAsync(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsJson()) return await httpResponseMessage.Content.ReadAsStringAsync();

            var errorResponse = await httpResponseMessage.ToDynamicsErrorResponseAsync();

            return errorResponse?.Error?.InnerError?.InternalException?.Message ??
                   errorResponse?.Error?.InnerError?.Message ??
                   errorResponse?.Error?.Message ??
                   await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public static async Task<bool> HasDynamicsErrorMessageAsync(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsJson()) return false;

            var errorResponse = await httpResponseMessage.ToDynamicsErrorResponseAsync();

            var hasValues = errorResponse?.Error?.InnerError?.InternalException?.Message != null || errorResponse?.Error?.InnerError?.Message != null || errorResponse?.Error?.Message != null;
            return hasValues;
        }

        /// <summary>
        /// Returns the http response message content as DynamicsErrorResponse if the content can be deserialized.
        /// If not able to deserialize, returns null
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<DynamicsErrorResponse> ToDynamicsErrorResponseAsync(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsJson()) return null;

            var content = await httpResponseMessage.Content.ReadAsStringAsync();

            try
            {
                var model = JsonConvert.DeserializeObject<DynamicsErrorResponse>(content);
                return model;
            }
            catch
            {
                return null;
            }
        }
    }
}