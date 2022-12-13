using Azure.Storage.Blobs.Specialized;
using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using CCBA.Integrations.DMF.Import.Models;
using CCBA.Integrations.DMF.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCBA.Integrations.DMF.Import
{
    /// <summary>
    /// Developer: Kobus Alberts, Johan Nieuwenhuis
    /// <example>
    /// Add to Startup Class and inject where needed.
    /// <code>
    /// [assembly: FunctionsStartup(typeof(Startup))]
    /// namespace Some.Name.Space
    /// {
    /// public class Startup : BaseStartup
    /// {
    /// public override void Configure(IFunctionsHostBuilder builder)
    /// {
    ///   builder.Services.AddTransient&lt;ImportService&gt;();
    /// }
    /// }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class ImportServiceWithD365Auth : BaseLogger
    {
        private readonly ODataD365FoService _oData365FoService;

        /// <summary>Initializes a new instance of the <see cref="ImportServiceWithD365Auth" /> class.</summary>
        public ImportServiceWithD365Auth(ODataD365FoService oData365FoService, ILogger<ImportServiceWithD365Auth> logger, IConfiguration configuration) : base(logger, configuration)
        {
            _oData365FoService = oData365FoService;
        }

        private enum ImportStage
        {
            GetAzureWriteUrl,
            ImportFromPackage,
            GetExecutionSummaryStatus,
            GetExecutionErrors,
            GenerateImportTargetErrorKeysFile,
            GetImportTargetErrorKeysFileUrl
        }

        public string ErrorMessage { get; set; }
        private string ApiBase { get; set; } = @"/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.";

        /// <summary>Queues the import.</summary>
        /// <param name="importConfiguration">The import configuration.</param>
        /// <param name="streamBytes">The stream bytes.</param>
        /// <param name="entityNames"></param>
        /// <returns>Return a task that hosts the DMFHttpResponse</returns>
        public async Task<DMFHttpResponse> QueueImport(ImportConfiguration importConfiguration, byte[] streamBytes, List<string> entityNames)
        {
            LogInformation("Start");

            var anyErrors = false;

            // 1. Get the Writable BlobUrl
            var root = new DMFHttpResponse();
            try
            {
                var post1 = await PostAsync(new AzureWritableURL { UniqueFileName = Guid.NewGuid().ToString() }, ImportStage.GetAzureWriteUrl);
                root.Status = post1.Status;
                if (post1.errorsExist)
                {
                    root.errorsExist = post1.errorsExist;
                    anyErrors = post1.errorsExist;
                }

                root.Add(post1);
                post1.Pi = root;

                // 2. Upload to BlobStorage
                await using (Stream stream = new MemoryStream(streamBytes))
                {
                    await new BlockBlobClient(new Uri(post1.packageFileUrl, UriKind.Absolute)).UploadAsync(stream);
                }

                // 3. Start import from package
                importConfiguration.packageUrl = post1.packageFileUrl;
                var post2 = await PostAsync(importConfiguration, ImportStage.ImportFromPackage);
                root.Status = post2.Status;
                if (post2.errorsExist)
                {
                    root.errorsExist = post2.errorsExist;
                    anyErrors = post2.errorsExist;
                }
                post1.Add(post2);
                post2.Pi = post1;
                var _executionId = post2.executionId;

                var errorFileUrls = new ConcurrentDictionary<string, string>();

                var httpResponseMessage = post2.httpResponseMessage;
                if (httpResponseMessage != null && httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string outPut;
                    var pollingTimer = 0;
                    const int maxPollingTime = 1800000;
                    //wait until process is complete
                    var importSummary = new Summary { ExecutionId = _executionId };
                    DMFHttpResponse post3;
                    do
                    {
                        const int pollingInterval = 5000;
                        if (pollingTimer >= 0)
                        {
                            await Task.Delay(pollingInterval);
                        }
                        pollingTimer += pollingInterval;

                        // 4. Check the execution status
                        post3 = await PostAsync(importSummary, ImportStage.GetExecutionSummaryStatus);

                        root.Status = post3.Status;
                        if (post3.errorsExist)
                        {
                            root.errorsExist = post3.errorsExist;
                            anyErrors = post3.errorsExist;
                        }
                        post2.Add(post3);
                        post3.Pi = post2;

                        outPut = post3.httpResponseMessage.StatusCode == HttpStatusCode.OK ? post3.responseMessage : "Executing";

                        if (outPut == "NotRun")
                        {
                            await Task.Delay(10000);
                        }

                        if (outPut == "Executing")
                        {
                            await Task.Delay(2000);
                        }
                    } while ((outPut == "NotRun" || outPut == "Executing") && pollingTimer <= maxPollingTime);

                    if (outPut != "Succeeded")
                    {
                        // 5. Get the execution errors
                        var post4 = await PostAsync(importSummary, ImportStage.GetExecutionErrors); //Todo:¯\_(ツ)_/¯
                        root.Status = post4.Status;
                        if (post4.errorsExist)
                        {
                            root.errorsExist = post4.errorsExist;
                            anyErrors = post4.errorsExist;
                        }
                        post3.Add(post4);
                        post4.Pi = post3;
                        ErrorMessage = post4.errorMessage;

                        var tasks = new List<Task>();

                        foreach (var entityName in entityNames)
                        {
                            var item = await Task.Factory.StartNew(async () =>
                            {
                                // 6. Generate Error File if errors exist
                                var importTargetErrorKeys = new ImportTargetErrorKeys
                                {
                                    ExecutionId = _executionId,
                                    EntityName = entityName
                                };

                                var post5 = await PostAsync(importTargetErrorKeys, ImportStage.GenerateImportTargetErrorKeysFile);
                                root.Status = post5.Status;
                                if (post5.errorsExist)
                                {
                                    root.errorsExist = post5.errorsExist;
                                    anyErrors = post5.errorsExist;
                                }
                                post4.Add(post5);
                                post5.Pi = post4;
                                if (post5.errorsExist)
                                {
                                    //  var attempts = 0;
                                    //There are errors in the import job
                                    DMFHttpResponse post6;
                                    do
                                    {
                                        const int pollingInterval = 5000;
                                        if (pollingTimer >= 0)
                                        {
                                            await Task.Delay(pollingInterval);
                                        }
                                        pollingTimer += pollingInterval;
                                        //attempts++;
                                        // 6. Generate Error File if errors exist
                                        post6 = await PostAsync(importTargetErrorKeys, ImportStage.GetImportTargetErrorKeysFileUrl);
                                        root.Status = post6.Status;
                                        if (post6.errorsExist)
                                        {
                                            root.errorsExist = post6.errorsExist;
                                            anyErrors = post6.errorsExist;
                                        }
                                        post5.Add(post6);
                                        post6.Pi = post5;

                                        if (!post6.httpResponseMessage.IsSuccessStatusCode) break;
                                    } while (!post6.ErrorFileUrls.Any() && pollingTimer <= maxPollingTime);

                                    foreach (var post6ErrorFileUrl in post6.ErrorFileUrls)
                                    {
                                        errorFileUrls.TryAdd(post6ErrorFileUrl.Key, post6ErrorFileUrl.Value);
                                    }
                                }
                            });

                            tasks.Add(item);
                        }

                        Task.WaitAll(tasks.ToArray());
                    }
                    else
                    {
                        post3.responseMessage = "Import job succeeded";
                    }
                }
                foreach (var errorFileUrl in errorFileUrls)
                {
                    root.ErrorFileUrls.Add(errorFileUrl.Key, errorFileUrl.Value);
                }
                root.errorMessage = ErrorMessage;
                if (ErrorMessage == "[]")
                {
                    LogInformation("No ErrorFileUrls returned!", LogLevel.Warning);
                    root.errorMessage = "No ErrorFileUrls returned!";
                }

                LogInformation("End");
            }
            catch (Exception e)
            {
                LogInformation($"Something went wrong. {e.GetAllMessages()}", LogLevel.Error);
                root.errorsExist = true;
                root.errorMessage = e.GetAllMessages();
            }

            root.errorsExist = anyErrors;
            return root;
        }

        /// <summary>Posts the asynchronous.</summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="payLoad">The pay load.</param>
        /// <param name="enumStage">The enum stage.</param>
        /// <returns>The task that hosts the DMFHttpResponse</returns>
        /// <exception cref="HttpRequestException"></exception>
        private async Task<DMFHttpResponse> PostAsync<TPayload>(TPayload payLoad, ImportStage enumStage) where TPayload : class, new()
        {
            var dMfHttpResponse = new DMFHttpResponse();

            var result = await _oData365FoService.PostAsync($"{ApiBase}{enumStage}", new StringContent(JsonConvert.SerializeObject(payLoad), Encoding.UTF8, "application/json"));
            var asStringAsync = await result.Content.ReadAsStringAsync();
            if (result.IsSuccessStatusCode)
            {
                //LogInformation($"{enumStage}: Success Status Code Received");
                var resultProp = JObject.Parse(asStringAsync).GetValue("value")?.ToString();
                if (!string.IsNullOrWhiteSpace(resultProp))
                {
                    switch (enumStage)
                    {
                        case ImportStage.GetAzureWriteUrl:
                            dMfHttpResponse.packageFileUrl = JObject.Parse(resultProp).GetValue("BlobUrl")?.ToString();
                            LogInformation($"{enumStage}: {dMfHttpResponse.packageFileUrl}");
                            break;

                        case ImportStage.ImportFromPackage:
                            dMfHttpResponse.executionId = resultProp;
                            LogInformation($"{enumStage}: {resultProp}");
                            break;

                        case ImportStage.GetExecutionSummaryStatus:
                            dMfHttpResponse.responseMessage = resultProp;
                            dMfHttpResponse.errorMessage += resultProp.Equals("Failed") ? "Failed" : "";
                            LogInformation($"{enumStage}: {resultProp}", resultProp.Equals("Failed") ? LogLevel.Critical : LogLevel.Information);
                            break;

                        case ImportStage.GetExecutionErrors:
                            dMfHttpResponse.errorMessage = resultProp;
                            if (!string.IsNullOrWhiteSpace(resultProp))
                            {
                                LogInformation($"{enumStage}: {resultProp}", LogLevel.Error);
                            }

                            break;

                        case ImportStage.GenerateImportTargetErrorKeysFile:
                            dMfHttpResponse.errorsExist = bool.Parse(resultProp);
                            if (dMfHttpResponse.errorsExist)
                            {
                                dMfHttpResponse.errorMessage += $"{enumStage}: Error Status Received ({dMfHttpResponse.errorsExist})";
                                LogInformation($"{enumStage}: Error Status Received ({dMfHttpResponse.errorsExist})", LogLevel.Error);
                            }

                            break;

                        case ImportStage.GetImportTargetErrorKeysFileUrl:
                            if (payLoad is ImportTargetErrorKeys t)
                            {
                                dMfHttpResponse.ErrorFileUrls.Add(t.EntityName, resultProp);
                                LogInformation($"{enumStage}: Error File Urls Received", LogLevel.Error);
                            }
                            break;
                    }
                }
                else
                {
                    LogInformation($"HTTP post:{enumStage} Result Empty", LogLevel.Warning);
                }
            }
            else
            {
                dMfHttpResponse.errorsExist = true;
                try
                {
                    var error = JsonConvert.DeserializeObject<DmfImportError>(asStringAsync);
                    dMfHttpResponse.errorMessage += JsonConvert.SerializeObject(error);
                    LogInformation(JsonConvert.SerializeObject(error), LogLevel.Error);
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            dMfHttpResponse.Status = result.StatusCode;
            dMfHttpResponse.httpResponseMessage = result;

            return dMfHttpResponse;
        }

        #region Utilities

        private static ConcurrentDictionary<Guid, Thread> Threads = new ConcurrentDictionary<Guid, Thread>();

        private void ThreadedAction(Action action)
        {
            LogInformation("Thread Created");
            var guid = Guid.NewGuid();
            var t = new Thread(() =>
                {
                    action.Invoke();
                    while (Threads.TryRemove(guid, out _))
                    {
                        Thread.Sleep(1000);
                    }
                })
            { IsBackground = true, Priority = ThreadPriority.Lowest };
            while (!Threads.TryAdd(guid, t))
            {
                Thread.Sleep(1);
            }
            t.Start();
            LogInformation("Thread Started");
        }

        #endregion Utilities
    }
}