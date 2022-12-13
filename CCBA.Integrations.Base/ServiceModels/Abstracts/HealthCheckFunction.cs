using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Models.HealthCheck;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    public abstract class HealthCheckFunction : AFunction
    {
        private static readonly HttpClient HttpClient = new();
        private readonly IServiceProvider _serviceProvider;

        private bool _synapseIncludeFields;

        protected HealthCheckFunction(ILogger<HealthCheckFunction> logger, IConfiguration configuration, IServiceProvider serviceProvider) : base(logger, configuration)
        {
            _serviceProvider = serviceProvider;
        }

        public Dictionary<string, HealthCheckConfigurationOptions> ConfigurationEntities { get; set; } = new();

        public List<HealthCheckEndpoint> Endpoints { get; set; } = new();
        public List<string> SynapseCeEntities { get; set; } = new();
        public List<string> SynapseFoEntities { get; set; } = new();

        public async Task<List<HealthCheckResponseItem>> SynapseCheck(string connectionString, IEnumerable<string> entities, CancellationToken cancellationSource)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

            var result = new ConcurrentBag<HealthCheckResponseItem>();

            Task.WaitAll(entities.Where(x => !string.IsNullOrEmpty(x)).Distinct()
                .Select(entity => Task.Run(async () =>
                {
                    var stopwatch = Stopwatch.StartNew();

                    var healthCheckItem = new HealthCheckResponseItem
                    {
                        Category = "Synapse",
                        Name = entity,
                        Endpoint = connectionString,
                        Database = sqlConnectionStringBuilder.InitialCatalog,
                        Properties = new Dictionary<string, object>()
                    };

                    try
                    {
                        await using var sqlConnection = new SqlConnection(connectionString);
                        healthCheckItem.Endpoint = sqlConnection.DataSource;

                        await sqlConnection.OpenAsync(cancellationSource);

                        var sql = $"SELECT TOP 0 * FROM {entity}";

                        await using var sqlCommand = new SqlCommand(sql, sqlConnection);
                        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationSource);

                        var fields = Enumerable.Range(0, sqlDataReader.FieldCount).Select(sqlDataReader.GetName).ToList();

                        healthCheckItem.Properties.Add("FieldCount", fields.Count);

                        if (_synapseIncludeFields) healthCheckItem.Properties.Add("Fields", fields);

                        healthCheckItem.Status = HealthCheckStatus.Ok;
                    }
                    catch (Exception e)
                    {
                        healthCheckItem.Status = HealthCheckStatus.Error;
                        healthCheckItem.Properties ??= new Dictionary<string, object>();
                        healthCheckItem.Properties.Add("Exception", e);
                    }

                    healthCheckItem.Duration = stopwatch.Elapsed;

                    result.Add(healthCheckItem);
                }, cancellationSource)).ToArray());

            return result.ToList();
        }

        protected override void Init()
        {
        }

        protected async Task<ActionResult> PerformHealthCheck(HttpRequest req, Assembly assembly, CancellationToken cancellationToken)
        {
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            GetParameters(req);

            var response = new HealthCheckResponse { Assembly = assembly.GetName().Name };

            CheckAssemblies(response);

            var taskConfiguration = Task.Run(() => { CheckConfiguration(response); }, cancellationSource.Token);

            var taskEndpoints = Task.Run(async () => { await CheckEndpoints(response); }, cancellationSource.Token);

            var taskSynapseFo = Task.Run(async () => { await CheckSynapseFo(response, cancellationSource); }, cancellationSource.Token);

            var taskSynapseCe = Task.Run(async () => { await CheckSynapseCe(response, cancellationSource); }, cancellationSource.Token);

            var taskD365Fo = Task.Run(async () => { await CheckD365Fo(cancellationSource, response); }, cancellationSource.Token);

            var taskD365Ce = Task.Run(async () => { await CheckD365Ce(cancellationSource, response); }, cancellationSource.Token);

            Task.WaitAll(new[] { taskConfiguration, taskEndpoints, taskSynapseFo, taskSynapseCe, taskD365Fo, taskD365Ce }, cancellationSource.Token);

            response.Status = HealthCheckStatus.Ok;

            if (response.Configuration != null && response.Configuration.Any(x => x.Status == HealthCheckStatus.Error)) response.Status = HealthCheckStatus.Error;
            if (response.Synapse != null && response.Synapse.Any(x => x.Status == HealthCheckStatus.Error)) response.Status = HealthCheckStatus.Error;
            if (response.Dynamics != null && response.Dynamics.Any(x => x.Status == HealthCheckStatus.Error)) response.Status = HealthCheckStatus.Error;
            if (response.Endpoints != null && response.Endpoints.Any(x => x.Status == HealthCheckStatus.Error)) response.Status = HealthCheckStatus.Error;

            LogInformation("Returning health-check response", properties: new Dictionary<string, string>
            {
                { "HealthCheck", JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) }
            });

            if (req.Headers["HealthCheckKey"] != Configuration["HealthCheckKey"])
            {
                return new OkObjectResult(new { Status = response.Status.ToString() }); // Return unauthorized response with status only
            }

            return new JsonResult(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });
        }

        private void CheckAssemblies(HealthCheckResponse response)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name != null && x.GetName().Name!.StartsWith("CCBA.Integrations", StringComparison.InvariantCulture));
                response.DomainAssemblies = new Dictionary<string, string>();

                foreach (var item in assemblies)
                {
                    response.DomainAssemblies.Add(item.GetName().Name!, item.GetName().Version?.ToString());
                }
            }
            catch (Exception e)
            {
                LogException(e.Message, e, LogLevel.Warning);
            }
        }

        private void CheckConfiguration(HealthCheckResponse response)
        {
            if (!ConfigurationEntities.Any()) return;

            foreach (var entity in ConfigurationEntities.Where(x => !string.IsNullOrEmpty(x.Key)))
            {
                var configurationCheck = new HealthCheckResponseItem
                {
                    Category = "Configuration",
                    Name = entity.Key,
                    Status = Configuration.GetValue<string>(entity.Key, null) != null ? HealthCheckStatus.Ok : HealthCheckStatus.Error
                };

                if (entity.Value.Visible)
                {
                    configurationCheck.Value = entity.Value.ValueType != null ? Configuration.GetValue(entity.Value.ValueType, entity.Key, null) : Configuration.GetValue<string>(entity.Key, null);
                }

                if (entity.Value.Optional) configurationCheck.Status = null;

                response.Configuration ??= new List<HealthCheckResponseItem>();
                response.Configuration.Add(configurationCheck);
            }
        }

        private async Task CheckD365Ce(CancellationTokenSource cancellationSource, HealthCheckResponse response)
        {
            var oDataD365CeService = _serviceProvider.GetService<ODataD365CeService>();
            if (oDataD365CeService != null)
            {
                oDataD365CeService.RetryPolicies.Clear();
                oDataD365CeService.RetryPolicyHandlers.Clear();

                var stopwatch = Stopwatch.StartNew();
                var d365CeCheck = new HealthCheckResponseItem { Category = "Dynamics", Name = "D365CE" };

                const string requestUri = "api/data/v9.2/ccba_calllists?$select=createdon,ccba_genesysqueuename,ccba_phonenumber,ccba_ordering&$expand=ccba_account($select=accountnumber)&$filter=statuscode eq 1 and Microsoft.Dynamics.CRM.Today(PropertyName=@p1)&@p1='createdon'";

                try
                {
                    var responseMessage = await oDataD365CeService.GetAsync(requestUri, cancellationSource.Token);
                    d365CeCheck.Status = responseMessage.IsSuccessStatusCode ? HealthCheckStatus.Ok : HealthCheckStatus.Error;
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        d365CeCheck.Properties = new Dictionary<string, object> { { "Exception", new Exception(await responseMessage.Content.ReadAsStringAsync()) } };
                    }
                }
                catch (Exception e)
                {
                    d365CeCheck.Status = HealthCheckStatus.Error;
                    d365CeCheck.Properties = new Dictionary<string, object> { { "Exception", e } };
                }

                d365CeCheck.Endpoint = oDataD365CeService.ApiBase;
                d365CeCheck.Duration = stopwatch.Elapsed;

                response.Dynamics ??= new List<HealthCheckResponseItem>();
                response.Dynamics.Add(d365CeCheck);
            }
        }

        private async Task CheckD365Fo(CancellationTokenSource cancellationSource, HealthCheckResponse response)
        {
            var oDataD365FoService = _serviceProvider.GetService<ODataD365FoService>();
            if (oDataD365FoService != null)
            {
                oDataD365FoService.RetryPolicies.Clear();
                oDataD365FoService.RetryPolicyHandlers.Clear();

                var stopwatch = Stopwatch.StartNew();
                var d365FoCheck = new HealthCheckResponseItem { Category = "Dynamics", Name = "D365FO" };

                const string requestUri = "/data/Customers?$format=json&$filter=CustomerAccount%20eq%20%27{{custAccount}}%27&$select=CustomerAccount,Name,AddressDescription,FullPrimaryAddress";

                try
                {
                    var responseMessage = await oDataD365FoService.GetAsync(requestUri, cancellationSource.Token);
                    d365FoCheck.Status = responseMessage.IsSuccessStatusCode ? HealthCheckStatus.Ok : HealthCheckStatus.Error;
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        d365FoCheck.Properties = new Dictionary<string, object> { { "Exception", new Exception(await responseMessage.Content.ReadAsStringAsync()) } };
                    }
                }
                catch (Exception e)
                {
                    d365FoCheck.Status = HealthCheckStatus.Error;
                    d365FoCheck.Properties = new Dictionary<string, object> { { "Exception", e } };
                }

                d365FoCheck.Endpoint = oDataD365FoService.ApiBase;
                d365FoCheck.Duration = stopwatch.Elapsed;

                response.Dynamics ??= new List<HealthCheckResponseItem>();
                response.Dynamics.Add(d365FoCheck);
            }
        }

        private async Task CheckEndpoints(HealthCheckResponse response)
        {
            if (!Endpoints.Any()) return;

            var actionBlock = new ActionBlock<HealthCheckEndpoint>(async endpoint =>
            {
                var stopwatch = Stopwatch.StartNew();
                var endpointCheck = new HealthCheckResponseItem { Category = "Endpoints", Name = endpoint.Name };
                endpointCheck.Properties ??= new Dictionary<string, object>();

                switch (endpoint)
                {
                    case HealthCheckTcpEndpoint tcpEndpoint:
                        endpointCheck.Properties ??= new Dictionary<string, object>();
                        endpointCheck.Properties.Add("Host", tcpEndpoint.Host);
                        endpointCheck.Properties.Add("Port", tcpEndpoint.Port);
                        endpointCheck.Endpoint = $"{tcpEndpoint.Host}:{tcpEndpoint.Port}";

                        try
                        {
                            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = true };
                            stopwatch.Restart();
                            await socket.ConnectAsync(tcpEndpoint.Host, tcpEndpoint.Port);
                            stopwatch.Stop();
                            socket.Close();
                            endpointCheck.Status = HealthCheckStatus.Ok;
                        }
                        catch (Exception e)
                        {
                            endpointCheck.Status = HealthCheckStatus.Error;
                            endpointCheck.Properties.Add("Exception", e);
                        }

                        endpointCheck.Duration = stopwatch.Elapsed;
                        break;

                    case HealthCheckHttpEndpoint httpEndpoint:
                        endpointCheck.Properties.Add("Address", httpEndpoint.Address);
                        endpointCheck.Properties.Add("Method", httpEndpoint.Method.ToString());
                        endpointCheck.Endpoint = httpEndpoint.Address;

                        try
                        {
                            if (httpEndpoint.Authentication is HealthCheckHttpEndpoint.AuthenticationTypeBasic authenticationTypeBasic)
                            {
                                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authenticationTypeBasic.Username}:{authenticationTypeBasic.Password}")));
                            }

                            var httpResponseMessage = await HttpClient.SendAsync(new HttpRequestMessage(httpEndpoint.Method, httpEndpoint.Address));
                            endpointCheck.Properties.Add("StatusCode", httpResponseMessage.StatusCode.ToString());

                            if (httpEndpoint.StatusCodeWhitelist == null || !httpEndpoint.StatusCodeWhitelist.Any())
                            {
                                endpointCheck.Status = httpResponseMessage.IsSuccessStatusCode ? HealthCheckStatus.Ok : HealthCheckStatus.Error;
                            }
                            else
                            {
                                endpointCheck.Status = httpEndpoint.StatusCodeWhitelist.Any(x => x == httpResponseMessage.StatusCode) ? HealthCheckStatus.Ok : HealthCheckStatus.Error;
                                endpointCheck.Properties.Add(nameof(httpEndpoint.StatusCodeWhitelist), httpEndpoint.StatusCodeWhitelist);
                            }
                        }
                        catch (Exception e)
                        {
                            endpointCheck.Status = HealthCheckStatus.Error;
                            endpointCheck.Properties.Add("Exception", e);
                        }

                        endpointCheck.Duration = stopwatch.Elapsed;
                        break;
                }

                response.Endpoints ??= new List<HealthCheckResponseItem>();
                response.Endpoints.Add(endpointCheck);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 16 });

            foreach (var endpoint in Endpoints)
            {
                actionBlock.Post(endpoint);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
        }

        private async Task CheckSynapseCe(HealthCheckResponse response, CancellationTokenSource cancellationSource)
        {
            if (SynapseCeEntities.Any())
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = string.IsNullOrWhiteSpace(Configuration["CE:DataLake:DataSource"]) ? Configuration["CEDataLakeDataSource"] : Configuration["CE:DataLake:DataSource"],
                    InitialCatalog = string.IsNullOrWhiteSpace(Configuration["CE:DataLake:InitialCatalog"]) ? Configuration["CEDataLakeInitialCatalog"] : Configuration["CE:DataLake:InitialCatalog"],
                    UserID = Configuration["ManagedIdentityId"],
                    Authentication = EnvironmentExtensions.IsDevelopment ? SqlAuthenticationMethod.ActiveDirectoryInteractive : SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                    CommandTimeout = 180
                };

                var synapseConnectionString = builder.ConnectionString;

                response.Synapse ??= new List<HealthCheckResponseItem>();
                response.Synapse.AddRange(await SynapseCheck(synapseConnectionString, SynapseCeEntities, cancellationSource.Token));
            }
        }

        private async Task CheckSynapseFo(HealthCheckResponse response, CancellationTokenSource cancellationSource)
        {
            if (SynapseFoEntities.Any())
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = string.IsNullOrWhiteSpace(Configuration["FO:DataLake:DataSource"]) ? Configuration["FODataLakeDataSource"] : Configuration["FO:DataLake:DataSource"],
                    InitialCatalog = string.IsNullOrWhiteSpace(Configuration["FO:DataLake:InitialCatalog"]) ? Configuration["FODataLakeInitialCatalog"] : Configuration["FO:DataLake:InitialCatalog"],
                    UserID = Configuration["ManagedIdentityId"],
                    Authentication = EnvironmentExtensions.IsDevelopment ? SqlAuthenticationMethod.ActiveDirectoryInteractive : SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                    CommandTimeout = 180
                };

                var synapseConnectionString = builder.ConnectionString;

                response.Synapse ??= new List<HealthCheckResponseItem>();
                response.Synapse.AddRange(await SynapseCheck(synapseConnectionString, SynapseFoEntities, cancellationSource.Token));
            }
        }

        private void GetParameters(HttpRequest req)
        {
            req.Headers.TryGetValue("synapse-include-fields", out var synapseIncludeFields);
            bool.TryParse(synapseIncludeFields.ToString(), out _synapseIncludeFields);
        }
    }
}