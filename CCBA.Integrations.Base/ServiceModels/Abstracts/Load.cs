using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public abstract class Load<TInput, TOutput> : ETL<TInput, TOutput>
    {
        protected Load(ILogger<Load<TInput, TOutput>> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }
    }
}