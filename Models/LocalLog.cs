namespace Goddard.Clock.Models
{
    public class LocalLog : LocalEntity
    {
        public DateTime Occurred { get; set; }
        public string? GloballyUniqueID { get; set; }
        public string? ExceptionData { get; set; }
        public string? HelpLink { get; set; }
        public int HResult { get; set; }
        public string? InnerExceptionID { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? Source { get; set; }
        public string? StackTrace { get; set; }
        public string? OptionalMessage { get; set; }
    }
}
