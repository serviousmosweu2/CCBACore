using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.ServiceModels.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.DMF.Export.Models
{
    public abstract class ExtractDmf : Extract<ExtractDmf.Input, string>
    {
        protected ExtractDmf(ILogger<ExtractDmf> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }

        public class Input
        {
            public Input(string definitionGroupId, string legalEntity, string fileName, string tempFileName)
            {
                DefinitionGroupId = definitionGroupId;
                LegalEntity = legalEntity;
                FileName = fileName;
                TempFileName = tempFileName;
            }

            public string DefinitionGroupId { get; set; }
            public string FileName { get; set; }
            public string LegalEntity { get; set; }
            public string TempFileName { get; set; }
        }
    }
}