using CCBA.Integrations.Base.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCBA.Integrations.Base.ServiceModels.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// Dependencies: <see cref="StopwatchService"/>
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class Transform<TInput, TOutput> : ETL<TInput, TOutput>
    {
        protected Transform(ILogger<Transform<TInput, TOutput>> logger, IConfiguration configuration, StopwatchService stopWatchService) : base(logger, configuration, stopWatchService)
        {
        }
    }
}