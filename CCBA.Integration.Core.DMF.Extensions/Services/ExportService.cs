using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CCBA.Infinity;
using CCBA.Integration.Core.DMF.Extensions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCBA.Integration.Core.DMF.Extensions.Services
{
    public class ExportService : IExportService
    {
        private int waitperiod = 5000;
        public ILogger<ExportService> _logger;
        private IOptions<D365FOApplicationSettings> _config;
        private HttpClient _client;
        private static string _exportErrorMessage = string.Empty;
        private static string _exportedFileUri = string.Empty;

        public ExportService(IHttpClientFactory clientFactory, IOptions<D365FOApplicationSettings> config, ILogger<ExportService> logger)
        {
            _logger = logger;
            _config = config;
            _client = clientFactory.CreateClient("D365FOAuthorizedClient");
        }

        private async Task<DMFHttpResponse> DoTask(ExportConfiguration export, string logHeader)
        {
            var dMfResponse = new DMFHttpResponse();
            try
            {
                if (export == null)
                {
                    throw new ArgumentNullException("Export Configuration is null");
                }
                if (string.IsNullOrEmpty(logHeader))
                {
                    throw new ArgumentNullException("logHeader can not be null");
                }
                var stringPayload = JsonConvert.SerializeObject(export);

                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                var result = _client.PostAsync("/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.ExportToPackage",
                        httpContent).Result;

                var resultContent = await result.Content.ReadAsStringAsync();

                dMfResponse.httpResponseMessage = result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var executionId = JObject.Parse(resultContent).GetValue("value").ToString();
                    string outPut = null;
                    do
                    {
                        result = await GetResult(executionId, "/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetExecutionSummaryStatus");
                        if (result.IsSuccessStatusCode)
                        {
                            outPut = JObject.Parse(await result.Content.ReadAsStringAsync()).GetValue("value").ToString();
                            dMfResponse.httpResponseMessage = result;
                        }
                        else
                        {
                            break;
                        }
                       

                    } while (outPut == "NotRun" || outPut == "Executing");


                    if (outPut != "Succeeded" && outPut != "PartiallySucceeded")
                    {
                        _logger.LogInformation($"{logHeader} Succeeded with errors: Getting errors from D365");
                        //Get execution errors;

                        result = await GetResult(executionId, "/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetExecutionErrors");

                        _exportErrorMessage = JObject.Parse(await result.Content.ReadAsStringAsync()).GetValue("value").ToString();

                        dMfResponse.exportErrorMessage = _exportErrorMessage;
                        _logger.LogInformation($"{logHeader} Export error: {_exportErrorMessage}");
                    }
                    else
                    {
                        _logger.LogInformation($"{logHeader} Succeeded: Retrieving downloadable URL");
                        // 3. Get downloable Url to download the package    

                        result = await GetResult(executionId, "/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.GetExportedPackageUrl");

                        _exportedFileUri = JObject.Parse(await result.Content.ReadAsStringAsync()).GetValue("value").ToString();

                        _logger.LogInformation($"{logHeader} Exported Url: {_exportedFileUri}");
                        dMfResponse.exportedFileUrl = _exportedFileUri;
                    }
                }

                return dMfResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"{logHeader} Error: {exception.Message}", exception);
                throw exception;
            }
        }

        private async Task<HttpResponseMessage> GetResult(string executionId, string executionsummarystatus)
        {
            return await _client.PostAsync(executionsummarystatus, new StringContent(JsonConvert.SerializeObject(new ImportSummary { ExecutionId = executionId }), Encoding.UTF8, "application/json"));
        }


        public async Task<IResult> ExportPackageAsync(ExportConfiguration exportSettings, string entryName, string logHeader)
        {
            try
            {
                if (exportSettings == null)
                {
                    throw new ArgumentNullException("Export Configuration is null");
                }
                if (string.IsNullOrEmpty(entryName))
                {
                    throw new ArgumentNullException("entryName can not be null");
                }
                if (string.IsNullOrEmpty(logHeader))
                {
                    throw new ArgumentNullException("logHeader can not be null");
                }
                _logger.LogInformation($"{logHeader} executing dmf task {JsonConvert.SerializeObject(exportSettings)}");

                //// Execute the intrinsic task for the export
                var dMfResponse = await DoTask(exportSettings, logHeader);

                if (dMfResponse.httpResponseMessage.StatusCode == HttpStatusCode.OK &&
                    !string.IsNullOrEmpty(dMfResponse.exportedFileUrl))
                {
                    _logger.LogInformation($"{logHeader} downloading file from blobstorage: {dMfResponse.exportedFileUrl}");
                    var blob = new BlobClient(new Uri(dMfResponse.exportedFileUrl));

                    await using var stream = new MemoryStream();
                    await blob.DownloadToAsync(stream);

                    _logger.LogInformation($"{logHeader} download complete for: {blob.Name}");

                    //Work on downloaded file
                    await using var zipToOpen = stream;
                    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);
                    var batchStream = archive.GetEntry(entryName)?.Open();

                    MemoryStream ms = new MemoryStream();
                    batchStream.CopyTo(ms);
                    if (ms == null)
                    {
                        throw new FileNotFoundException($"{logHeader} Entry for {entryName} not found in package.");
                    }

                    _logger.LogInformation($"{logHeader} extracted package: {archive.Entries.Count} files");
                    return new Result(ms, dMfResponse.httpResponseMessage.StatusCode, dMfResponse.exportedFileUrl);
                }
                else
                {
                    _logger.LogError($"{logHeader} Error Response : {dMfResponse.httpResponseMessage.Content.ReadAsStringAsync()}");
                    throw new Exception(await dMfResponse.httpResponseMessage.Content.ReadAsStringAsync());
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error Response : {exception.Message}", exception);
                throw exception;
            }
        }
    }
}