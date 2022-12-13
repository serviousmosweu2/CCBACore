using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class ExtractLegalEntity : BaseLogger
    {
        protected ExtractLegalEntity(ILogger<ExtractLegalEntity> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        protected internal List<string> LegalEntities { get; set; }
    }
}