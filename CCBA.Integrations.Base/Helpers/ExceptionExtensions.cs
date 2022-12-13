using System;

namespace CCBA.Integrations.Base.Helpers
{
    public static class ExceptionExtensions
    {
        public static void EnsureNotNull(this object o, string argumentName)
        {
            if (o == null) throw new NullReferenceException($"{argumentName} is null");
        }
    }
}