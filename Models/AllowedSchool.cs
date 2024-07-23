using SQLite;

namespace Goddard.Clock.Models
{
    public class AllowedSchool : LocalEntity
    {
        public AllowedSchool()
            : base()
        { }
        [Indexed]
        public string? State { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
    }
}
