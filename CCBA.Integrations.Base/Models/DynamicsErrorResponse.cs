using Newtonsoft.Json;

namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Represents an error response received from Dynamics
    /// </summary>
    public class DynamicsErrorResponse
    {
        public DynamicsError Error { get; set; }

        public class DynamicsError
        {
            public string Code { get; set; }
            public DynamicsInnerError InnerError { get; set; }
            public string Message { get; set; }

            public class DynamicsInnerError
            {
                public DynamicsInternalException InternalException { get; set; }
                public string Message { get; set; }

                [JsonIgnore]
                public string Stacktrace { get; set; }

                public string Type { get; set; }

                public class DynamicsInternalException
                {
                    public string Message { get; set; }
                    public string Stacktrace { get; set; }
                    public string Type { get; set; }
                }
            }
        }
    }
}