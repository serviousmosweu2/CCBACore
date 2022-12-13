using CCBA.Integrations.Base.Enums;

namespace CCBA.Integrations.Base.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class BaseGeneralLogger : BaseJobLogger
    {
        protected BaseGeneralLogger(string interfaceName, string source, string target, string jobRunId, string programId, string legalEntity, string method) : base(interfaceName, source, target, jobRunId, programId, legalEntity, method)
        {
            MessageType = $"integration-job-{EMessageType.Status.ToString().ToLower()}";
        }

        public string JobStatus { get; set; }
    }
}