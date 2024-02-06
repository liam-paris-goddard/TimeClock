using SQLite;
using System;
using System.Runtime.Serialization;

namespace TimeClock.Models
{
    public class Child : LocalEntity, IPerson
    {
        public Child() : base() { }

        [Indexed]
        public long PID { get; init; }

        [Indexed]
        public string FN { get; init; }

        [Indexed]
        public string LN { get; init; }

        public string Sex { get; init; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname => $"{LN}, {FN}";

        //note cannot be unique index due to children existing on multiple parent IDs
        [Indexed]
        public long PersonID { get; init; }

        [Indexed]
        public long FamilyID { get; init; }

        public string Classroom { get; init; }
    }
}