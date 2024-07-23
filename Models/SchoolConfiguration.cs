using SQLite;

namespace Goddard.Clock.Models
{
    public class SchoolConfiguration : LocalEntity
    {
        public SchoolConfiguration()
            : base()
        { }
        [Indexed]
        public long SchoolID { get; set; }
        public string? Name { get; set; }
        public bool BypassSignatureParents { get; set; }
        public bool BypassSignatureEmployees { get; set; }
        public int TimezoneOffset { get; set; }
        public bool ObservesDST { get; set; }
    }
}
