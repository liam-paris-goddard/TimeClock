using SQLite;
using System;
using System.Runtime.Serialization;

namespace TimeClock.Models
{
    public class Employee : LocalEntity, IPerson
    {
        public Employee() : base() { }

        [Indexed]
        public string FN { get; init; }

        [Indexed]
        public string LN { get; init; }

        [Indexed]
        public string PIN { get; init; }

        public DateTime? ForceResetPIN { get; init; }

        public bool LockedPIN { get; init; }

        public bool AllowChildClockInOut { get; init; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname => $"{LN}, {FN}";

        [Ignore]
        [IgnoreDataMember]
        public string FriendlyName => $"{FN} {LN}";

        [Indexed(Unique = true)]
        [Unique]
        public long PersonID { get; init; }
    }
}