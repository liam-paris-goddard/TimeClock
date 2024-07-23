using SQLite;
using System.Runtime.Serialization;

namespace Goddard.Clock.Models
{
    public enum ClockEventType
    {
        In = 0,
        Out = 1
    }

    public class Event : LocalEntity
    {
        public Event()
            : base()
        { }
        [Indexed]
        public ClockEventType Type { get; set; }
        [Indexed]
        public UserType UserType { get; set; }
        [Indexed]
        public DateTime Occurred { get; set; }
        [Indexed]
        public long TargetPersonID { get; set; }
        [Indexed]
        public long UserPersonID { get; set; }
        [IgnoreDataMember]
        public string? ImageKey { get; set; }

        public byte[]? Signature { get; set; }
    }

    public class EventExtended : LocalEntity
    {
        [Indexed]
        public ClockEventType Type { get; set; }
        [Indexed]
        public UserType UserType { get; set; }
        [Indexed]
        public DateTime Occurred { get; set; }
        [Indexed]
        public long TargetPersonID { get; set; }
        [Indexed]
        public long UserPersonID { get; set; }
        public string? UserPersonName { get; set; }
        public string? TargetPersonName { get; set; }

        public string ExplanationText
        {
            get { return Type == ClockEventType.In ? "Clocked-In" : "Clocked-Out"; }
        }

        public Event ConvertToEvent()
        {
            return new Models.Event()
            {
                Type = this.Type,
                UserType = this.UserType,
                Occurred = this.Occurred,
                TargetPersonID = this.TargetPersonID,
                UserPersonID = this.UserPersonID
            };
        }
    }
}
