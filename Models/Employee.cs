using SQLite;
using System;
using System.Runtime.Serialization;

namespace TimeClock.Models
{
    public class Employee : LocalEntity, IPerson
    {
        public Employee() : base() { }

        [Indexed]
        public string FN { get; set; }

        [Indexed]
        public string LN { get; set; }

        [Indexed]
        public string PIN { get; set; }

        public DateTime? ForceResetPIN { get; set; }

        public bool LockedPIN { get; set; }

        public bool AllowChildClockInOut { get; set; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname => $"{LN}, {FN}";

        [Ignore]
        [IgnoreDataMember]
        public string FriendlyName => $"{FN} {LN}";

        [Indexed(Unique = true)]
        [Unique]
        public long PersonID { get; set; }
    }
}