using System;

namespace CCBA.Infinity
{
    public class LogData
    {
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string loglevel { get; set; }
        public string errorcode { get; set; }
        public string message { get; set; }
        public Job job { get; set; }

    }
}