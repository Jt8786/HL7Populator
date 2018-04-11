using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HL7Populator.Core
{
    public abstract class Person
    {
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Cell { get; set; }
        public string Sex { get; set; }

        protected Person()
        {
            PopulateData(RandomPersonGenerator.GetRandomResult(1).Results[0]);
        }

        protected Person(RandomPersonGenerator.Result result)
        {
            PopulateData(result);
        }

        private void PopulateData(RandomPersonGenerator.Result result)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            Prefix = textInfo.ToTitleCase(result.Name.Title);
            LastName = textInfo.ToTitleCase(result.Name.Last);
            FirstName = textInfo.ToTitleCase(result.Name.First);
            Address = textInfo.ToTitleCase(result.Location.Street);
            ZipCode = result.Location.Postcode.ToString();
            City = textInfo.ToTitleCase(result.Location.City);
            State = textInfo.ToTitleCase(result.Location.State);
            Cell = result.Cell;
            Phone = result.Phone;
            DateOfBirth = Convert.ToDateTime(result.Dob);
            Sex = textInfo.ToTitleCase(result.Gender);
        }

        public static class RandomPersonGenerator
        {
            private const string url = "https://randomuser.me/api/1.1/?nat=us&results={0}&exc=email,login,registered,picture,id";

            public static RandomResult GetRandomResult(int peopleCount)
            {
                var request = WebRequest.Create(string.Format(url, peopleCount));
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse)request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<RandomResult>(text);
            }

            public class Name
            {
                public string Title { get; set; }
                public string First { get; set; }
                public string Last { get; set; }
            }

            public class Location
            {
                public string Street { get; set; }
                public string City { get; set; }
                public string State { get; set; }
                public int Postcode { get; set; }
            }

            public class Result
            {
                public string Gender { get; set; }
                public Name Name { get; set; }
                public Location Location { get; set; }
                public string Dob { get; set; }
                public string Phone { get; set; }
                public string Cell { get; set; }
                public string Nat { get; set; }
            }

            public class Info
            {
                public string Seed { get; set; }
                public int Results { get; set; }
                public int Page { get; set; }
                public string Version { get; set; }
            }

            public class RandomResult
            {
                public List<Result> Results { get; set; }
                public Info Info { get; set; }
            }
        }
    }
}
