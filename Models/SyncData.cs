using SQLite;

namespace Goddard.Clock.Models
{
    public class PullSyncData
    {
        public Parent[]? Parents { get; set; }
        public Child[]? Children { get; set; }
        public Employee[]? Employees { get; set; }

        public PullSyncData(IEnumerable<Parent> parents, IEnumerable<Child> children, IEnumerable<Employee> employees)
            : this()
        {
            Parents = parents?.ToArray();
            Children = children?.ToArray();
            Employees = employees?.ToArray();
        }

        public PullSyncData()
        { }

    }

    public class SendSyncData
    {
        public Event[]? Events { get; set; }
        public UpdatePIN[]? UpdatePINs { get; set; }

        public SendSyncData(IEnumerable<Event> events, IEnumerable<UpdatePIN> updatePINs)
            : this()
        {
            Events = events?.ToArray();
            UpdatePINs = updatePINs?.ToArray();
        }

        public SendSyncData()
        { 
        Events = new Event[0];
        UpdatePINs = new UpdatePIN[0];
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
        public LogType Type { get; set; }


        [Indexed]
        public DateTime Occurred { get; set; }
    }
}
