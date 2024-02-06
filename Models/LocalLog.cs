using System;

namespace TimeClock.Models
{
    public class LocalLog : LocalEntity
    {
        public DateTime Occurred { get; init; }
        public string GloballyUniqueID { get; init; }
        public string ExceptionData { get; init; }
        public string HelpLink { get; init; }
        public int HResult { get; init; }
        public string InnerExceptionID { get; init; }
        public string ExceptionMessage { get; init; }
        public string Source { get; init; }
        public string StackTrace { get; init; }
        public string OptionalMessage { get; init; }
    }
}