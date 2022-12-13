using Azure.Storage.Blobs;
using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using CCBA.Integrations.DMF.Export.Interfaces;
using CCBA.Integrations.DMF.Export.Models;
using CCBA.Integrations.DMF.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CCBA.Integrations.DMF.Export
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
    ///   builder.Services.AddTransient&lt;ExportService&gt;();
    /// }
    /// }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class ExportServiceWithD365Auth : BaseLogger
    {
        private readonly ODataD365FoService _oDataD365FoService;

        public ExportServiceWithD365Auth(ODataD365FoService oDataD365FoService, ILogger<BaseLogger> logger, IConfiguration configuration) : base(logger, configuration)
        {
            _oDataD365FoService = oDataD365FoService;
        }

        private enum ExportStage
        {
            ExportToPackage,
            GetExecutionSummaryStatus,
            GetExecutionErrors,
            GetExportedPackageUrl
        }

        private string ApiBase { get; set; } = @"/data/DataManagementDefinitionGroups/Microsoft.Dynamics.DataEntities.";

        /// <summary>Exports the package asynchronous.</summary>
        /// <param name="exportSettings">The export settings.</param>
        /// <param name="entryName">Name of the entry.</param>
        /// <returns>The result for the extraction of file or files from the package</returns>
        /// <exception cref="ArgumentNullException">
        ///     Export Configuration is null
        ///     or
        ///     entryName can not be null
        /// </exception>
        /// <exception cref="FileNotFoundException">Entry for {entryName} not found in package.</exception>
        /// <exception cref="Exception"></exception>
        public async Task<IDMFResult> ExportPackageAsync(ExportConfiguration exportSettings, string entryName)
        {
            LogInformation("Begin");
            try
            {
                if (exportSettings == null) throw new ArgumentNullException("Export Configuration is null");
                if (string.IsNullOrEmpty(entryName)) throw new ArgumentNullException("entryName can not be null");

                //// Execute the intrinsic task for the export
                var dMfResponse = await ExecuteWorkflowTask(exportSettings);

                if (dMfResponse.httpResponseMessage.StatusCode == HttpStatusCode.OK &&
                    !string.IsNullOrEmpty(dMfResponse.exportedFileUrl))
                {
                    await using var stream = new MemoryStream();
                    await new BlobClient(new Uri(dMfResponse.exportedFileUrl)).DownloadToAsync(stream);

                    //Work on downloaded file
                    await using var zipToOpen = stream;
                    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);
                    if (archive.Entries.All(s => s.FullName != entryName))
                    {
                        LogInformation("File not found! Entry does not exist.");
                        throw new Exception("File not found! Entry does not exist.");
                    }
                    var batchStream = archive.GetEntry(entryName)?.Open();

                    await using var ms = new MemoryStream();
                    if (batchStream != null) await batchStream.CopyToAsync(ms);
                    if (ms == null) throw new FileNotFoundException($"Entry for {entryName} not found in package.");

                    var bytes = ms.ToArray();

                    var deBom = Encoding.Default.GetBytes(RemoveBom(Encoding.Default.GetString(bytes)));

                    LogInformation("End");
                    //var resultUri = await BlobExtensions.SaveToBlobContainer(deBom, $@"{exportSettings.definitionGroupId}_{entryName}_{DateTime.Now:yyyyMMdd HH-mm-ss}", exportSettings.definitionGroupId);
                    //dMfResponse.packageFileUrl = resultUri.ToString();
                    return new DMFResult(deBom, dMfResponse.httpResponseMessage.StatusCode, dMfResponse.exportedFileUrl);
                }
                LogInformation("End");
                throw new Exception(await dMfResponse.httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch (Exception exception)
            {
                AppExceptionLogger(exception.GetAllMessages(), EErrorCode.DMF, LogLevel.Critical, exception);
                throw exception;
            }
        }

        private static string RemoveBom(string p)
        {
            var BOMMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (p.StartsWith(BOMMarkUtf8, StringComparison.OrdinalIgnoreCase))
                p = p.Remove(0, BOMMarkUtf8.Length);
            return p.Replace("\0", "");
        }

        /// <summary>Executes the workflow task.</summary>
        /// <param name="export">The export configuration</param>
        /// <returns>Returns task for DMFHttpResponse<br /></returns>
        /// <exception cref="ArgumentNullException">Export Configuration is null</exception>
        private async Task<DMFHttpResponse> ExecuteWorkflowTask(ExportConfiguration export)
        {
            try
            {
                if (export == null) throw new ArgumentNullException("Export Configuration is null");

                var dMfResponse = await PostAsync(export, ExportStage.ExportToPackage, export.definitionGroupId);

                if (dMfResponse.Status == HttpStatusCode.OK)
                {
                    var payLoad = new Summary { ExecutionId = dMfResponse.executionId };
                    do
                    {
                        dMfResponse = await PostAsync(payLoad, ExportStage.GetExecutionSummaryStatus, export.definitionGroupId);
                        if (dMfResponse.responseMessage == "NotRun")
                        {
                            await Task.Delay(10000);
                        }

                        if (dMfResponse.responseMessage == "Executing")
                        {
                            await Task.Delay(2000);
                        }
                    } while (dMfResponse.responseMessage == "NotRun" || dMfResponse.responseMessage == "Executing");

                    if (dMfResponse.responseMessage != "Succeeded" && dMfResponse.responseMessage != "PartiallySucceeded")
                        dMfResponse = await PostAsync(payLoad, ExportStage.GetExecutionErrors, export.definitionGroupId);
                    else
                        dMfResponse = await PostAsync(payLoad, ExportStage.GetExportedPackageUrl, export.definitionGroupId);
                }

                return dMfResponse;
            }
            catch (Exception exception)
            {
                AppExceptionLogger($"{export?.definitionGroupId}: {exception.GetAllMessages()}", EErrorCode.DMF, LogLevel.Critical, exception);
                throw;
            }
        }

        /// <summary>Posts the asynchronous.</summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="payLoad">The pay load.</param>
        /// <param name="dMFHttpResponse">The Data Management Framework HTTP response.</param>
        /// <param name="enumStage">The enum stage.</param>
        /// <returns>The task for the DMFHttpResponse<br /></returns>
        /// <exception cref="HttpRequestException"></exception>
        private async Task<DMFHttpResponse> PostAsync<TPayload>(TPayload payLoad, ExportStage enumStage, string exportDefinitionGroupId)
        {
            var dMFHttpResponse = new DMFHttpResponse();
            var result = await _oDataD365FoService.PostAsync($"{ApiBase}{enumStage}", new StringContent(JsonConvert.SerializeObject(payLoad), Encoding.UTF8, "application/json"));
            if (result.IsSuccessStatusCode)
            {
                //LogInformation($"Started HTTP post:{enumStage}");
                var resultProp = JObject.Parse(await result.Content.ReadAsStringAsync()).GetValue("value")?.ToString();
                switch (enumStage)
                {
                    case ExportStage.ExportToPackage:
                        dMFHttpResponse.executionId = resultProp;
                        LogInformation($"{exportDefinitionGroupId} {enumStage}: {resultProp}");
                        break;

                    case ExportStage.GetExecutionSummaryStatus:
                        dMFHttpResponse.responseMessage = resultProp;
                        LogInformation($"{exportDefinitionGroupId} {enumStage}: {resultProp}");
                        break;

                    case ExportStage.GetExecutionErrors:
                        dMFHttpResponse.errorMessage = resultProp;
                        LogInformation($"{exportDefinitionGroupId} {enumStage}: {resultProp}");
                        break;

                    case ExportStage.GetExportedPackageUrl:
                        dMFHttpResponse.exportedFileUrl = resultProp;
                        LogInformation($"{exportDefinitionGroupId} {enumStage}: {resultProp}");
                        break;
                }

                dMFHttpResponse.Status = HttpStatusCode.OK;
                dMFHttpResponse.httpResponseMessage = result;
                //LogInformation($"Completed HTTP post:{enumStage}");
            }
            else
            {
                var errorString = await result.Content.ReadAsStringAsync();
                LogInformation($"{exportDefinitionGroupId}: {errorString}");

                throw new HttpRequestException(errorString);
            }

            return dMFHttpResponse;
        }
    }
}