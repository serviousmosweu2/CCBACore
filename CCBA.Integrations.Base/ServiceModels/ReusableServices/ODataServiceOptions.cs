namespace CCBA.Integrations.Base.ServiceModels.ReusableServices
{
    /// <summary>
    /// Developer: Konrad Steynberg
    /// </summary>
    public class ODataServiceOptions
    {
        public ODataServiceOptions(string clientName, string apiBase)
        {
            ClientName = clientName;
            ApiBase = apiBase;
        }

        public string ApiBase { get; }
        public string ClientName { get; }

        /// <summary>
        /// Retry count
        /// </summary>
        public int RetryCount { get; set; } = 15;

        /// <summary>
        /// Exponential retry delay in milliseconds (factor)
        /// </summary>
        public int RetryDelay { get; set; } = 50;

        public bool RetryIsExponential { get; set; } = true;
        public bool UseAuthorization { get; set; } = true;

        public bool UseRetryPolicy { get; set; } = true;
    }
}