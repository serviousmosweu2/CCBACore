using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Enums;

namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class AppFailureLogger : BaseGeneralLogger
    {
        public AppFailureLogger(string jobRunId, string programId, string legalEntity, string interfaceName,
            string source, string target, string method) : base(interfaceName, source, target, jobRunId, programId, legalEntity, method)
        {
            JobStatus = EJobStatus.Failure.ToString();
        }
    }
}