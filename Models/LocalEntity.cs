using SQLite;
using System;
using System.Runtime.Serialization;

namespace TimeClock.Models
{
    public abstract class LocalEntity
    {
        [PrimaryKey, AutoIncrement]
        public long ID { get; init; }

        [IgnoreDataMember]
        public DateTime Inserted { get; init; }

        [IgnoreDataMember]
        public DateTime Updated { get; init; }

        [IgnoreDataMember]
        public bool IsDeleted { get; init; }

        [IgnoreDataMember]
        public DateTime? Uploaded { get; init; }

        public LocalEntity()
        {
            IsDeleted = false;
            Inserted = DateTime.Now;
            Updated = DateTime.Now;
        }
    }

    public interface IPerson
    {
        long PersonID { get; init; }
        string LN { get; init; }
        string FN { get; init; }
    }
}