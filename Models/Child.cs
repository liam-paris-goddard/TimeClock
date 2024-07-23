using SQLite;
using System.Runtime.Serialization;

namespace Goddard.Clock.Models
{
    public class Child : LocalEntity, IPerson
    {
        public Child()
            : base()
        { }
        [Indexed]
        public long? PID { get; set; }
        [Indexed]
        public string? FN { get; set; }
        [Indexed]
        public string? LN { get; set; }
        public string? Sex { get; set; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname
        {
            get { return String.Format("{0}, {1}", LN, FN); }
        }

        //note cannot be unique index due to children existing on multiple parent IDs
        [Indexed]
        public long PersonID { get; set; }
        [Indexed]
        public long FamilyID { get; set; }

        public string? Classroom { get; set; }
    }
}
