using System;

namespace CCBA.Integrations.Base.Models
{
    public class DataLakeException : Exception
    {
        public DataLakeException()
        {
        }

        public DataLakeException(string message) : base(message)
        {
        }

        public DataLakeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}