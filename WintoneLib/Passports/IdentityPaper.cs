using System;
using System.Collections.Generic;

namespace WintoneLib.Passports
{
    public class IdentityPaper
    {
        public string IdentityType { get; set; }
        public string IdentityNo { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }
        public string Gender { get; set; }

        public string Nationality { get; set; }
        public DateTime? Birthday { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime? SignDate { get; set; }
        public string SignOffice { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public static IdentityPaper CreateFromDictionay(Dictionary<string, string> dict)
        {
            var result = new IdentityPaper();

            if (dict == null || dict.Count == 0) return result;

            //result.IdentityNo = dict.GetValueOrDefault("");

            return result;
        }

    }
}
