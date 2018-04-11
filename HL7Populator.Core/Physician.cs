using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.Core
{
    public class Physician : Person
    {
        public string HospitalID { get; set; }

        public Physician() : base()
        {
            Initialize(string.Empty);
        }
        public Physician(string hospitalIDPrefix) : base()
        {
            Initialize(hospitalIDPrefix);
        }

        private Physician(RandomPersonGenerator.Result result) : base(result)
        {
            Initialize(string.Empty);
        }

        private Physician(string hospitalIDPrefix, RandomPersonGenerator.Result result) : base(result)
        {
            Initialize(hospitalIDPrefix);
        }

        public static List<Physician> GetRandomPhysicians(int count)
        {
            List<Physician> physicians = new List<Physician>();

            foreach (var result in RandomPersonGenerator.GetRandomResult(count).Results)
            {
                Physician patient = new Physician(result);
                physicians.Add(patient);
            }

            return physicians;
        }
        
        private void Initialize(string hospitalIDPrefix)
        {
            HospitalID = HospitalIDGenerator.GetNewHospitalID(hospitalIDPrefix);
            Prefix = "Dr.";
            Suffix = "MD";
        }

        private static class HospitalIDGenerator
        {
            private static readonly object _object = new object();
            private static int currentMessageID = new Random().Next(0, 100000);

            public static string GetNewHospitalID(string hospitalIDPrefix)
            {
                int messageID = 0;

                lock (_object)
                {
                    messageID = currentMessageID++;
                }

                return string.Format("{0}{1}{2}", hospitalIDPrefix, DateTime.Now.ToString("yyMMdd"), messageID.ToString("00000"));
            }
        }
    }
}
