using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    public abstract class Extract<TInput, TOutput> : ETL<TInput, TOutput>
    {
        protected Extract(ILogger<Extract<TInput, TOutput>> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }
    }
}