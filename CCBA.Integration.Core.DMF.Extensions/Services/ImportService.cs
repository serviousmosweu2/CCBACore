using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using CCBA.Infinity;
using CCBA.Integration.Core.DMF.Extensions.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCBA.Integration.Core.DMF.Extensions.Services
{
    public class ImportService : IImportService
    {
        static string errorFiledownloadUrl = string.Empty;
        static string executionID = string.Empty;
        private ILogger<ImportService> _logger;
        private HttpClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="logger"></param>
        public ImportService(IHttpClientFactory clientFactory, ILogger<ImportService> logger)
        {
            _logger = logger;
            _client = clientFactory.CreateClient("D365FOAuthorizedClient");
        }

        /// <summary>
        /// Retrieves the Azure writable Url
        /// </summary>
        /// <returns>Returns a HttpResponseMessage task</returns>
        private async Task<HttpResponseMessage> GetAzureWritableUrlAsync()
        {
            var stringPayload = JsonConvert.SerializeObject(new AzureWritableURL()
            {
                UniqueFileName = new Guid().ToString()
            });
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage result = await _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetAzureWriteUrl", httpContent);
            return result;
        }

        /// <summary>
        /// Enqueue the Data package to Recurring integration
        /// </summary>
        /// <returns>Status</returns>
        public async Task<DMFHttpResponse> QueueImport(ImportConfiguration importConfiguration, byte[] streamBytes, string entityName)
        {
            DMFHttpResponse dmfResponse = new DMFHttpResponse();
            Stream stream = new MemoryStream(streamBytes);

            var result = await GetAzureWritableUrlAsync();
            string resultContent = await result.Content.ReadAsStringAsync();
            string packageUrl = string.Empty;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                string azResult = JObject.Parse(resultContent).GetValue("value").ToString();
                packageUrl = JObject.Parse(azResult).GetValue("BlobUrl").ToString();
            }
            else
            {
                dmfResponse.httpResponseMessage = result;
                dmfResponse.responseMessage = "Failed to retrieve Azure writable URL.";
                return dmfResponse;
            }

            var blob = new BlockBlobClient(new Uri(packageUrl, UriKind.Absolute));
            await blob.UploadAsync(stream);

            importConfiguration.packageUrl = packageUrl;

            var stringPayload = JsonConvert.SerializeObject(importConfiguration);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.ImportFromPackage", httpContent).Result;
            resultContent = await result.Content.ReadAsStringAsync();

            //Set http response variable on the dmfResponse object
            dmfResponse.httpResponseMessage = result;

            _logger.LogInformation($"Response is {resultContent}");

            if (result.StatusCode == HttpStatusCode.OK)
            {
                //set execution ID
                executionID = JObject.Parse(resultContent).GetValue("value").ToString();
                _logger.LogInformation("Initiating import of a data project complete.");

                string outPut;
                int pollingTimer = 0;
                int maxPollingTime = 1800000;
                //wait until process is complete
                do
                {
                    _logger.LogInformation("Waiting for package to execution to complete");
                    int pollingInterval = 5000;

                    Thread.Sleep(pollingInterval);
                    pollingTimer += pollingInterval;
                    _logger.LogInformation("Checking status...");

                    //set API parameters for GetExecutionSummaryStatus
                    stringPayload = JsonConvert.SerializeObject(new ImportSummary
                    {
                        ExecutionId = executionID
                    });
                    httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetExecutionSummaryStatus", httpContent).Result;
                    resultContent = await result.Content.ReadAsStringAsync();

                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        resultContent = await result.Content.ReadAsStringAsync();
                        outPut = JObject.Parse(resultContent).GetValue("value").ToString();

                        //set hhtp response on dmfResponse object
                        dmfResponse.httpResponseMessage = result;
                        _logger.LogInformation("Status of import is :" + outPut);
                    }
                    else
                    {
                        outPut = "Executing";
                        _logger.LogInformation($"Status of import id: NotRun / Executing.Checking again in {pollingInterval} seconds");
                    }

                }
                while ((outPut == "NotRun" || outPut == "Executing") && pollingTimer <= maxPollingTime);

                if (outPut != "Succeeded" && outPut != "PartiallySucceeded")
                {
                    resultContent = await result.Content.ReadAsStringAsync();
                    _logger.LogInformation(resultContent);

                    Thread.Sleep(5000);

                    //Get all the errors
                    stringPayload = JsonConvert.SerializeObject(new ImportSummary
                    {
                        ExecutionId = executionID
                    });

                    httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetExecutionErrors", httpContent).Result;
                    var errorContent = await result.Content.ReadAsStringAsync();


                    //Generate Error File if errors exist
                    stringPayload = JsonConvert.SerializeObject(new ImportTargetErrorKeys() { ExecutionId = executionID, EntityName = entityName });
                    httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GenerateImportTargetErrorKeysFile", httpContent).Result;
                    resultContent = await result.Content.ReadAsStringAsync();
                    bool errorsExist = bool.Parse(JObject.Parse(resultContent).GetValue("value").ToString());

                    if (errorsExist)
                    {
                        //There are errors in the import job
                        do
                        {
                            //Fetch error file
                            _logger.LogInformation("Fetching Error File...");

                            Thread.Sleep(5000);

                            result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetImportTargetErrorKeysFileUrl", httpContent).Result;
                            resultContent = await result.Content.ReadAsStringAsync();
                            outPut = JObject.Parse(resultContent).GetValue("value").ToString();

                        }
                        while (outPut == string.Empty);

                        errorFiledownloadUrl = JObject.Parse(resultContent).GetValue("value").ToString();

                    }

                    _logger.LogInformation(resultContent);
                    dmfResponse.Status = "400";

                    dmfResponse.httpResponseMessage = result;
                    dmfResponse.errorFileUrl = errorFiledownloadUrl;
                    dmfResponse.responseMessage = errorContent;
                    _logger.LogInformation(errorContent);

                }
                else
                {
                    dmfResponse.Status = "200";
                    dmfResponse.responseMessage = "Import job succeeded";
                }
            }
            else
            {
                dmfResponse.Status = "502";
                dmfResponse.responseMessage = resultContent;
                _logger.LogInformation($"Initiating import of a data project...Failed");
                _logger.LogInformation(resultContent);
            }
            return dmfResponse;
        }
    }
}