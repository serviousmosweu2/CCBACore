using CCBA.Integrations.Base.Enums;
using CCBA.Integrations.Base.Helpers;
using CCBA.Integrations.Base.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CCBA.Integrations.Base.Abstracts
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis, Konrad Steynberg
    /// </summary>
    public abstract class BaseLogger : BaseConfiguration
    {
        private readonly ILogger<BaseLogger> _logger;

        protected BaseLogger(ILogger<BaseLogger> logger, IConfiguration configuration) : base(configuration)
        {
            _logger = logger;
        }

        public string InterfaceName { get; set; }

        private string JobRunId { get; set; }
        private string LegalEntity { get; set; }
        private string ProgramId { get; set; }

        protected void AppExceptionLogger(string message, EErrorCode errorCode, LogLevel logLevel, Exception exception, string source = "", string target = "", List<JobData> jobData = null, [CallerMemberName] string method = "", Dictionary<string, string> properties = null)
        {
            var logger = new Logger(JobRunId, ProgramId, LegalEntity, InterfaceName, source, target, method, message, logLevel, jobData, errorCode, exception);

            properties ??= new Dictionary<string, string>();
            properties["ErrorCode"] = errorCode.ToString();
            properties["MessageType"] = logger.MessageType;
            properties["Job"] = JsonConvert.SerializeObject(logger.Job, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            LogException(message, exception, logLevel, source, target, jobData, method, properties);
        }

        protected void AppFailureLogger(string source = "", string target = "", [CallerMemberName] string method = "")
        {
            var logger = new AppFailureLogger(JobRunId, ProgramId, LegalEntity, InterfaceName, source, target, method);

            LogInformation("App Failure", LogLevel.Critical, source, target, new List<JobData>(), method, new Dictionary<string, string>
            {
                { "Job", JsonConvert.SerializeObject(logger.Job, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
                { "JobStatus", logger.JobStatus },
                { "MessageType", logger.MessageType },
            });
        }

        protected void AppStartLogger(string jobRunId, string programId, string legalEntity, string source = "", string target = "", [CallerMemberName] string method = "")
        {
            JobRunId = jobRunId;
            ProgramId = programId;
            LegalEntity = legalEntity;

            var logger = new AppStartLogger(JobRunId, ProgramId, LegalEntity, InterfaceName, source, target, method);

            LogInformation("App Start", LogLevel.Information, source, target, new List<JobData>(), method, new Dictionary<string, string>
            {
                { "Job", JsonConvert.SerializeObject(logger.Job, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
                { "JobStatus", logger.JobStatus },
                { "MessageType", logger.MessageType },
            });
        }

        protected void AppSuccessLogger(string source = "", string target = "", [CallerMemberName] string method = "")
        {
            var logger = new AppSuccessLogger(JobRunId, ProgramId, LegalEntity, InterfaceName, source, target, method);

            LogInformation("App Success", LogLevel.Information, source, target, new List<JobData>(), method, new Dictionary<string, string>
            {
                { "Job", JsonConvert.SerializeObject(logger.Job, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
                { "JobStatus", logger.JobStatus },
                { "MessageType", logger.MessageType },
            });
        }

        protected void LogException(Exception exception, LogLevel logLevel = LogLevel.Critical, string source = "", string target = "", List<JobData> jobData = null, [CallerMemberName] string method = "", Dictionary<string, string> properties = null)
        {
            LogException(exception.Message, exception, logLevel, source, target, jobData, method, properties);
        }

        protected void LogException(string message, Exception exception, LogLevel logLevel = LogLevel.Critical, string source = "", string target = "", List<JobData> jobData = null, [CallerMemberName] string method = "", Dictionary<string, string> properties = null)
        {
            properties ??= new Dictionary<string, string>();
            properties["Exception"] = JsonConvert.SerializeObject(exception, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var formattedMessage = BuildFormattedMessage($"{message}\n{exception.GetAllMessages()}", source, target, jobData, method, properties);

            _logger.Log(logLevel, exception, formattedMessage, properties.Select(x => (object)x.Value).ToArray());
        }

        protected void LogInformation(string message = "", LogLevel logLevel = LogLevel.Information, string source = "", string target = "", List<JobData> jobData = null, [CallerMemberName] string method = "", Dictionary<string, string> properties = null)
        {
            LogTrace(method, message, logLevel, source, target, jobData, properties);
        }

        protected void LogTrace(string method, string message, LogLevel logLevel, string source = "", string target = "", List<JobData> jobData = null, Dictionary<string, string> properties = null)
        {
            properties ??= new Dictionary<string, string>();

            var formattedMessage = BuildFormattedMessage(message, source, target, jobData, method, properties);

            _logger.Log(logLevel, formattedMessage, properties.Select(x => (object)x.Value).ToArray());
        }

        /// <summary>
        /// Replaces content with blob url if content is too big to fit in application insights property field
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string GetApplicationInsightsSafeValue(string content)
        {
            if (content == null) return null;
            return content.Length > 8192 ? BlobExtensions.UploadBlobWithSasUri("payloads", $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}", new BinaryData(content)) : content;
        }

        private string BuildFormattedMessage(string message, string source, string target, List<JobData> jobData, string method, Dictionary<string, string> properties)
        {
            if (!string.IsNullOrEmpty(JobRunId)) properties["JobRunId"] = JobRunId;
            if (!string.IsNullOrEmpty(ProgramId)) properties["ProgramId"] = ProgramId;
            if (!string.IsNullOrEmpty(InterfaceName)) properties["InterfaceName"] = InterfaceName;
            if (!string.IsNullOrEmpty(LegalEntity)) properties["LegalEntity"] = LegalEntity;
            if (!string.IsNullOrEmpty(source)) properties["Source"] = source;
            if (!string.IsNullOrEmpty(target)) properties["Target"] = target;
            if (!string.IsNullOrEmpty(method)) properties["Method"] = method;
            if (jobData != null) properties["JobData"] = JsonConvert.SerializeObject(jobData);

            foreach (var (key, value) in properties.ToList()) properties[key] = GetApplicationInsightsSafeValue(value);

            message ??= "(null)";
            message = message.Replace("{", "{{").Replace("}", "}}");

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{message.Trim().TrimEnd('.')}. ");
            foreach (var (key, _) in properties) stringBuilder.Append($"{key}={{{key}}} ");
            var formattedMessage = stringBuilder.ToString();
            return formattedMessage;
        }
    }
}