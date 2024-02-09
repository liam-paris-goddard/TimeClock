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
        public UserType UserType { get; set; }
        public Action Action { get; set; }
        public long UserID { get; set; }
        public string? Old { get; set; }
        public string? New { get; set; }
        public bool Locked { get; set; }
    }
}