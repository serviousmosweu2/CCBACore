using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="DictionaryService"/>
    /// </summary>
    public abstract class ExtractDataLakeSequential : Extract<ExtractDataLakeSequential.Input, Dictionary<string, List<Dictionary<string, object>>>>
    {
        private readonly DictionaryService _dictionaryService;

        protected ExtractDataLakeSequential(ILogger<ExtractDataLakeSequential> logger, IConfiguration configuration, StopwatchService stopWatchService, DictionaryService dictionaryService) : base(logger, configuration, stopWatchService)
        {
            _dictionaryService = dictionaryService;
        }

        protected string DataLakeConnectionString { get; set; }

        protected override async Task<Dictionary<string, List<Dictionary<string, object>>>> Execute(Input input)
        {
            var taskManager = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var keyValuePair in input.Sql)
            {
                LogInformation($@"Starting retrieving data from data lake for {keyValuePair.Key}");
                taskManager[keyValuePair.Key] = await _dictionaryService.GetPopulatedDictionary(keyValuePair.Value, DataLakeConnectionString);
                LogInformation($@"Retrieving data from data lake for {keyValuePair.Key} Completed.");
            }

            return taskManager;
        }

        public class Input
        {
            public Input(Dictionary<string, string> sql)
            {
                Sql = sql;
            }

            public Dictionary<string, string> Sql { get; set; }
        }
    }
}