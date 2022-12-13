using CCBA.Integrations.Base.Abstracts;
using CCBA.Integrations.Base.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class Logger : BaseJobLogger
    {
        public Logger(string jobRunId, string programId, string legalEntity, string interfaceName, string source, string target, string method, string message, LogLevel loglevel,
            List<JobData> jobData, EErrorCode errorCode = EErrorCode.None, Exception exception = null) : base(interfaceName, source, target, jobRunId, programId, legalEntity, method, exception)
        {
            LogLevel = loglevel.ToString();
            ErrorCode = errorCode == EErrorCode.None ? null : errorCode.ToString();
            Message = message;
            JobData = jobData;
            MessageType = $"integration-job-{EMessageType.Log.ToString().ToLower()}";
        }

        public string ErrorCode { get; }
        public List<JobData> JobData { get; }
        public string LogLevel { get; }
        public string Message { get; }
    }
}