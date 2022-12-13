using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// Dependencies: <see cref="ODataServiceOptions"/>, <see cref="IOAuthService"/>, <see cref="IHttpClientFactory"/>
    /// </summary>
    public class ODataService : BaseLogger
    {
        public readonly HttpClient HttpClient;
        public readonly List<IAsyncPolicy> RetryPolicies = new();
        public readonly List<Func<HttpResponseMessage, Task>> RetryPolicyHandlers = new();
        private readonly IOAuthService _oAuthService;
        private readonly ODataServiceOptions _oDataServiceOptions;

        public ODataService(ILogger<ODataService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ODataServiceOptions oDataServiceOptions, IOAuthService oAuthService = null) : base(logger, configuration)
        {
            oDataServiceOptions ??= new ODataServiceOptions(null, null);
            _oDataServiceOptions = oDataServiceOptions;

            _oAuthService = oAuthService;

            HttpClient = _oDataServiceOptions.ClientName == null ? httpClientFactory.CreateClient() : httpClientFactory.CreateClient(_oDataServiceOptions.ClientName);

            if (!string.IsNullOrEmpty(_oDataServiceOptions.ApiBase)) HttpClient.BaseAddress = new Uri(_oDataServiceOptions.ApiBase);

            if (_oDataServiceOptions.UseRetryPolicy)
            {
                // Create default policy
                var waitAndRetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(_oDataServiceOptions.RetryCount,
                attempt => TimeSpan.FromMilliseconds(_oDataServiceOptions.RetryIsExponential ? _oDataServiceOptions.RetryDelay * Math.Pow(2, attempt) : _oDataServiceOptions.RetryDelay),
                async (exception, timeSpan, retryCount, context) => { await ExceptionHandler(exception, timeSpan, retryCount, context); });

                // Add default policy
                RetryPolicies.Add(waitAndRetryPolicy);

                // Add default handler
                RetryPolicyHandlers.Add(async message => { await PolicyHandleRetryAfter(message); });
                RetryPolicyHandlers.Add(async message => { await PolicyHandle429(message); });
                RetryPolicyHandlers.Add(async message => { await PolicyHandle503(message); });
            }
        }

        public string ApiBase => _oDataServiceOptions.ApiBase;
        // These are different to policies (custom code runs)

        private IAsyncPolicy WrappedPolicies
        {
            get
            {
                if (!_oDataServiceOptions.UseRetryPolicy) return Policy.NoOpAsync();

                var asyncPolicies = RetryPolicies.ToArray();
                return RetryPolicies.Count switch
                {
                    0 => Policy.NoOpAsync(),
                    1 => asyncPolicies.First(),
                    _ => Policy.WrapAsync(asyncPolicies)
                };
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().DeleteAsync(requestUri, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(DeleteAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().GetAsync(requestUri, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(GetAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().GetAsync(requestUri, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(GetAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            await ExecuteResponseInspection(httpResponseMessage);

            var content = RemoveBom(await httpResponseMessage.Content.ReadAsStringAsync());
            var deserializeObject = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings { Error = JsonErrorHandler });
            return deserializeObject;
        }

        public async Task<HttpResponseMessage> PatchAsync(string action, HttpContent httpContent, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().PatchAsync(action, httpContent, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(PatchAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task PolicyHandle429(HttpResponseMessage result)
        {
            if (!_oDataServiceOptions.UseRetryPolicy) return;

            if (result.StatusCode == HttpStatusCode.TooManyRequests)
            {
                LogInformation($"{GetType().Name} handler {nameof(PolicyHandle429)} matched", LogLevel.Trace);
                result.EnsureSuccessStatusCode();
            }
        }

        public async Task PolicyHandle503(HttpResponseMessage result)
        {
            if (!_oDataServiceOptions.UseRetryPolicy) return;

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                LogInformation($"{GetType().Name} handler {nameof(PolicyHandle503)} matched", LogLevel.Trace);
                result.EnsureSuccessStatusCode();
            }
        }

        public async Task PolicyHandleRetryAfter(HttpResponseMessage result)
        {
            if (!_oDataServiceOptions.UseRetryPolicy) return;

            if (result.Headers?.RetryAfter?.Delta.HasValue != null)
            {
                var delta = result.Headers?.RetryAfter?.Delta;
                if (delta.HasValue)
                {
                    LogInformation($"{GetType().Name} handler {nameof(PolicyHandleRetryAfter)} matched", LogLevel.Trace, properties: new Dictionary<string, string>
                    {
                        { "Delta", result.Headers?.RetryAfter?.Delta.ToString() }
                    });
                    await Task.Delay(delta.Value);
                }
            }
        }

        public async Task PolicyHandleRetryAfterEnsureSuccess(HttpResponseMessage result)
        {
            if (!_oDataServiceOptions.UseRetryPolicy) return;

            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch
            {
                LogInformation($"{GetType().Name} handler {nameof(PolicyHandleRetryAfterEnsureSuccess)} matched", LogLevel.Trace);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsJson(string action, object content, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().PostAsJsonAsync(action, content, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(PatchAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().PostAsync(requestUri, httpContent, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(PatchAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent httpContent, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                var result = await GetHttpClient().PutAsync(requestUri, httpContent, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(PatchAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await WrappedPolicies.ExecuteAsync(async (context, token) =>
            {
                /*
                HttpRequestMessage has a limitation in that the message can only be sent once.
                For polly to work we need to recreate the object every time or we will get an error on subsequent tries: Cannot send the same request message multiple times.
                */
                var requestMessage = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);
                requestMessage.Content = httpRequestMessage.Content;

                requestMessage.Headers.Clear();
                foreach (var (key, value) in requestMessage.Headers.ToList()) requestMessage.Headers.Add(key, value);

                requestMessage.Properties.Clear();
                foreach (var (key, value) in httpRequestMessage.Properties) requestMessage.Properties.Add(key, value);

                var result = await GetHttpClient().SendAsync(requestMessage, cancellationToken);
                await InvokeRetryPolicyHandlers(result);
                return result;
            }, new Context(nameof(PatchAsync), new Dictionary<string, object> { { "StartedAt", DateTime.UtcNow } }), cancellationToken);

            return await ExecuteResponseInspection(httpResponseMessage);
        }

        protected virtual async Task ExceptionHandler(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            var totalDuration = default(TimeSpan);

            if (context.ContainsKey("StartedAt"))
            {
                context.TryGetValue("StartedAt", out var startedAtObject);
                DateTime.TryParse(startedAtObject?.ToString(), out var startedAt);
                totalDuration = DateTime.UtcNow - startedAt;
            }

            LogInformation(exception.Message, LogLevel.Warning, properties: new Dictionary<string, string>
            {
                { "TimeSpan", timeSpan.ToString() },
                { "RetryCount", retryCount.ToString() },
                { "ExceptionType", exception.GetType().ToString() },
                { "Context", JsonConvert.SerializeObject(context) },
                { "TotalDuration", totalDuration.ToString() }
            });
        }

        private static string RemoveBom(string p)
        {
            var bomMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (p.StartsWith(bomMarkUtf8, StringComparison.OrdinalIgnoreCase))
                p = p.Remove(0, bomMarkUtf8.Length);
            return p.Replace("\0", string.Empty);
        }

        /// <summary>
        /// Inspect response message before returning
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> ExecuteResponseInspection(HttpResponseMessage httpResponseMessage)
        {
            try
            {
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var hasDynamicsErrorMessage = await httpResponseMessage.HasDynamicsErrorMessageAsync();

                    if (hasDynamicsErrorMessage)
                    {
                        var dynamicsResponse = await httpResponseMessage.ToDynamicsErrorResponseAsync();
                        LogInformation("Dynamics error response", properties: new Dictionary<string, string>
                        {
                            { "ResponseObject", JsonConvert.SerializeObject(dynamicsResponse) }
                        });
                    }
                    else
                    {
                        LogInformation("Odata error response", properties: new Dictionary<string, string>
                        {
                            { "Content", await httpResponseMessage.Content.ReadAsStringAsync() }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e.Message, e, LogLevel.Warning);
            }

            return httpResponseMessage;
        }

        private HttpClient GetHttpClient()
        {
            if (_oDataServiceOptions.UseAuthorization && _oAuthService != null)
            {
                var accessToken = _oAuthService.GetCachedAccessToken();
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return HttpClient;
        }

        private async Task InvokeRetryPolicyHandlers(HttpResponseMessage result)
        {
            if (!_oDataServiceOptions.UseRetryPolicy) return;
            foreach (var retryHandler in RetryPolicyHandlers)
            {
                await retryHandler.Invoke(result);
            }
        }

        private void JsonErrorHandler(object sender, ErrorEventArgs e)
        {
            LogException(e.ErrorContext.Error);
            throw e.ErrorContext.Error;
        }
    }
}