using CCBA.Integrations.Base.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class StopwatchService : BaseLogger
    {
        private Stopwatch _stopwatch;

        public StopwatchService(ILogger<StopwatchService> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        public void Continue(string message, [CallerMemberName] string caller = "")
        {
            LogInformation($"Stopwatch - {message}", properties: new Dictionary<string, string>
            {
                { "Method", caller } ,
                {"Duration",_stopwatch.ElapsedMilliseconds.ToString()}
            });
            _stopwatch.Start();
        }

        /// <summary>
        /// Takes a sample of the current elapsed time from a running stopwatch.
        /// </summary>
        /// <param name="caller"></param>
        public void Sample(string message, [CallerMemberName] string caller = "")
        {
            LogInformation($"Stopwatch - {message}", properties: new Dictionary<string, string>
            {
                { "Method", caller } ,
                {"Duration",_stopwatch.ElapsedMilliseconds.ToString()}
            });
        }

        /// <summary>
        /// Resets and Starts Stopwatch
        /// </summary>
        /// <param name="caller"></param>
        public void Start(string message, [CallerMemberName] string caller = "")
        {
            LogInformation($"Stopwatch - {message}", properties: new Dictionary<string, string> { { "Method", caller } });
            GetStopwatch();
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        public void Stop(string message, [CallerMemberName] string caller = "")
        {
            _stopwatch.Stop();
            LogInformation($"Stopwatch - {message}", properties: new Dictionary<string, string>
            {
                { "Method", caller } ,
                {"Duration",_stopwatch.ElapsedMilliseconds.ToString()}
            });
        }

        private void GetStopwatch()
        {
            _stopwatch ??= new Stopwatch();
        }
    }
}