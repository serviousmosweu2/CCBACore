using System;

namespace CCBA.Integrations.Base.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public abstract class BaseJobLogger
    {
        protected BaseJobLogger(string interfaceName, string source, string target, string jobRunId, string programId, string legalEntity, string method, Exception exception = null)
        {
            Job = new JobBase(interfaceName, source, target, jobRunId, programId, legalEntity, method, exception);
        }

        public JobBase Job { get; }

        public string MessageType { get; set; }

        public class JobBase
        {
            public JobBase(string interfaceName, string source, string target, string jobRunId, string programId, string legalEntity, string method, Exception exception = null)
            {
                JobRunId = jobRunId;
                InterfaceName = interfaceName;
                Source = source;
                Target = target;
                ProgramId = programId;
                LegalEntity = legalEntity;
                ExceptionMessage = exception?.Message;
                StackTrace = exception?.StackTrace;
                Method = method;
            }

            public string ExceptionMessage { get; }
            public string InterfaceName { get; }
            public string JobRunId { get; }
            public string LegalEntity { get; set; }
            public string Method { get; set; }
            public string ProgramId { get; }
            public string Source { get; }
            public string StackTrace { get; }
            public string Target { get; }
        }
    }
}