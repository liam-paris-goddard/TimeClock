using Microsoft.Maui.Controls;
using SQLite;
using System;

namespace TimeClock.Models
{
    public class AllowedSchool : LocalEntity
    {
        public AllowedSchool() : base() { }

        [Indexed]
        public string? State { get; set; }

        public string? Name { get; set; }

        public string? Number { get; set; }
    }
}