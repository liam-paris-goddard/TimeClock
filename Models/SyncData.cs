using System;
using System.Linq;
using SQLite;

namespace TimeClock.Models
{
    public class PullSyncData
    {
        public Parent[] Parents { get; init; }
        public Child[] Children { get; init; }
        public Employee[] Employees { get; init; }

        public PullSyncData(IEnumerable<Parent> parents, IEnumerable<Child> children, IEnumerable<Employee> employees)
        {
            Parents = parents?.ToArray();
            Children = children?.ToArray();
            Employees = employees?.ToArray();
        }
    }

    public class SendSyncData
    {
        public Event[] Events { get; init; }
        public UpdatePIN[] UpdatePINs { get; init; }

        public SendSyncData(IEnumerable<Event> events, IEnumerable<UpdatePIN> updatePINs)
        {
            Events = events?.ToArray();
            UpdatePINs = updatePINs?.ToArray();
        }
    }

    public enum LogType
    {
        Pull = 0,
        Send = 1
    }

    public class LocalSyncLog : LocalEntity
    {
        [Indexed]
        public LogType Type { get; init; }

        [Indexed]
        public DateTime Occurred { get; init; }
    }
}