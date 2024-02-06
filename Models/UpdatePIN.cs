using System;

namespace TimeClock.Models
{
    public enum UserType
    {
        Parent = 0,
        Employee = 1,
        InLocoParentis = 2
    }

    public enum Action
    {
        Change = 0,
        Lock = 1
    }

    public class UpdatePIN : LocalEntity
    {
        public UserType UserType { get; init; }
        public Action Action { get; init; }
        public long UserID { get; init; }
        public string Old { get; init; }
        public string New { get; init; }
        public bool Locked { get; init; }
    }
}