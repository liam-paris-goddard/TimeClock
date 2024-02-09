using System;
using System.Linq;
using SQLite;

namespace TimeClock.Models
{
    public class PullSyncData(IEnumerable<Parent> parents, IEnumerable<Child> children, IEnumerable<Employee> employees)
    {
        public Parent[] Parents { get; set; } = parents?.ToArray() ?? [];
        public Child[] Children { get; set; } = children?.ToArray() ?? [];
        public Employee[] Employees { get; set; } = employees?.ToArray() ?? [];
    }


    public class SendSyncData(IEnumerable<Event> events, IEnumerable<UpdatePIN> updatePINs)
    {
        public Event[] Events { get; set; } = events?.ToArray() ?? [];
        public UpdatePIN[] UpdatePINs { get; set; } = updatePINs?.ToArray() ?? [];
    }

    public enum LogType
    {
        Pull = 0,
        Send = 1
    }

    public class LocalSyncLog : LocalEntity
    {
        [Indexed]
        public LogType Type { get; set; }

        [Indexed]
        public DateTime Occurred { get; set; }
    }
}