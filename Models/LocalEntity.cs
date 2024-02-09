using SQLite;
using System;
using System.Runtime.Serialization;

namespace TimeClock.Models
{
    public abstract class LocalEntity
    {
        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }

        [IgnoreDataMember]
        public DateTime Inserted { get; set; }

        [IgnoreDataMember]
        public DateTime Updated { get; set; }

        [IgnoreDataMember]
        public bool IsDeleted { get; set; }

        [IgnoreDataMember]
        public DateTime? Uploaded { get; set; }

        public LocalEntity()
        {
            IsDeleted = false;
            Inserted = DateTime.Now;
            Updated = DateTime.Now;
        }
    }

    public interface IPerson
    {
        long? PersonID { get; set; }
        string? LN { get; set; }
        string? FN { get; set; }
    }
}