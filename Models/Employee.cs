using SQLite;
using System.Runtime.Serialization;

namespace Goddard.Clock.Models
{
    public class Employee : LocalEntity, IPerson
    {
        public Employee()
            : base()
        { }
        [Indexed]
        public string? FN { get; set; }
        [Indexed]
        public string? LN { get; set; }
        [Indexed]
        public string? PIN { get; set; }
        public DateTime? ForceResetPIN { get; set; }
        public bool LockedPIN { get; set; }
        public bool AllowChildClockInOut { get; set; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname
        {
            get { return String.Format("{0}, {1}", LN, FN); }
        }

        [Ignore]
        [IgnoreDataMember]
        public string FriendlyName
        {
            get { return String.Format("{0} {1}", FN, LN); }
        }

        [Indexed(Unique = true)]
        [Unique]
        public long PersonID { get; set; }
    }
}
