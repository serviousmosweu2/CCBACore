using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.ReusableServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>, <see cref="DictionaryService"/>
    /// </summary>
    public abstract class ExtractDataLake : Extract<ExtractDataLake.Input, Dictionary<string, Task<List<Dictionary<string, object>>>>>
    {
        private readonly DictionaryService _dictionaryService;

        protected ExtractDataLake(ILogger<ExtractDataLake> logger, IConfiguration configuration, DictionaryService dictionaryService, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
            _dictionaryService = dictionaryService;
        }

        protected string DataLakeConnectionString { get; set; }

        protected override async Task<Dictionary<string, Task<List<Dictionary<string, object>>>>> Execute(Input input)
        {
            var taskManager = new Dictionary<string, Task<List<Dictionary<string, object>>>>();
            foreach (var keyValuePair in input.Sql)
            {
                taskManager[keyValuePair.Key] = _dictionaryService.GetPopulatedDictionary(keyValuePair.Value, DataLakeConnectionString);
            }
            await Task.WhenAll(taskManager.Select(s => s.Value));
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