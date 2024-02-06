using SQLite;
using System;

namespace TimeClock.Models
{
    public enum ClockEventType
    {
        In = 0,
        Out = 1
    }

    public class Event : LocalEntity
    {
        public Event() : base() { }

        [Indexed]
        public ClockEventType Type { get; init; }

        [Indexed]
        public UserType UserType { get; init; }

        [Indexed]
        public DateTime Occurred { get; init; }

        [Indexed]
        public long TargetPersonID { get; init; }

        [Indexed]
        public long UserPersonID { get; init; }

        public byte[] Signature { get; init; }
    }

    public class EventExtended : LocalEntity
    {
        [Indexed]
        public ClockEventType Type { get; init; }

        [Indexed]
        public UserType UserType { get; init; }

        [Indexed]
        public DateTime Occurred { get; init; }

        [Indexed]
        public long TargetPersonID { get; init; }

        [Indexed]
        public long UserPersonID { get; init; }

        public string UserPersonName { get; init; }

        public string TargetPersonName { get; init; }

        public string ExplanationText => Type == ClockEventType.In ? "Clocked-In" : "Clocked-Out";

        public Event ConvertToEvent()
        {
            return new Event
            {
                Type = Type,
                UserType = UserType,
                Occurred = Occurred,
                TargetPersonID = TargetPersonID,
                UserPersonID = UserPersonID
            };
        }
    }
}