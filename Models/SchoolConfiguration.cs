using SQLite;
using System;

namespace TimeClock.Models
{
    public class SchoolConfiguration : LocalEntity
    {
        public SchoolConfiguration() : base() { }

        [Indexed]
        public long SchoolID { get; init; }

        public string Name { get; init; }

        public bool BypassSignatureParents { get; init; }

        public bool BypassSignatureEmployees { get; init; }

        public int TimezoneOffset { get; init; }

        public bool ObservesDST { get; init; }
    }
}