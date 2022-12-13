using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Enums;

namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class AppSuccessLogger : BaseGeneralLogger
    {
        public AppSuccessLogger(string jobRunId, string programId, string legalEntity, string interfaceName,
            string source, string target, string method) : base(interfaceName, source, target, jobRunId, programId, legalEntity, method)
        {
            JobStatus = EJobStatus.Success.ToString();
        }
    }
}