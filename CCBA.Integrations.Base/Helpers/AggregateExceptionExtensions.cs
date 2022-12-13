using System;

namespace CCBA.Integrations.Base.Helpers
{
    public static class AggregateExceptionExtensions
    {
        public static string GetAllMessages(this Exception exception)
        {
            var em = string.Empty;
            var ex = exception;
            while (ex != null)
            {
                em += Environment.NewLine + ex.Message;
                ex = ex.InnerException;
            }

            return em;
        }
    }
}