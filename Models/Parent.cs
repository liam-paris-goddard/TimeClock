using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock.Models
{
    public class Parent : LocalEntity, IPerson
    {
        public Parent()
            : base()
        { }
        [Indexed]
        public string? FN { get; set; }
        [Indexed]
        public string? LN { get; set; }
        [Indexed]
        public string? PIN { get; set; }

        public DateTime? ResetPIN { get; set; }
        public bool LockedPIN { get; set; }

        [Ignore]
        [IgnoreDataMember]
        public string Fullname
        {
            get { return String.Format("{0}, {1}", LN, FN); }
        }

        [Indexed(Unique = true)]
        [Unique]
        public long? PersonID { get; set; }
        [Indexed]
        public long FamilyID { get; set; }

        //the canonicalized name, uppercased, stripped of non-alpha characters, used for partial name matches
        //done this way for performance (vs "live" query of LN field)
        [Indexed]
        public string? CN { get; set; }
    }


    public static class Extensions
    {
        public static string CanonicalizeName(this string input)
        {
            if (String.IsNullOrWhiteSpace(input))
                return input;

            var arr = input.ToCharArray();

            //supposedly 3 times faster than using regex
            arr = Array.FindAll<char>(arr, (c => (char.IsLetter(c))));

            var results = new string(arr).ToUpper().Trim();
            if (results.Length >= 3)
                return results.Substring(0, 3);
            else
                return results;
        }
    }
}
