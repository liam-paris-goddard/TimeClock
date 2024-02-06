using Microsoft.Maui.Controls;
using SQLite;
using System;

namespace TimeClock.Models
{
    public class AllowedSchool : LocalEntity
    {
        public AllowedSchool() : base() { }

        [Indexed]
        public string State { get; init; }

        public string Name { get; init; }

        public string Number { get; init; }
    }
}