using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.Core
{
    public class Patient : Person
    {
        public string MRN { get; set; }
        public string DeptNumber { get; set; }
        public string MPI { get; set; }

        public Patient() : base()
        {
            Initialize(string.Empty);
        }

        public Patient(string mrnPrefix) : base()
        {
            Initialize(mrnPrefix);
        }

        private Patient(RandomPersonGenerator.Result result) : base(result)
        {
            Initialize(string.Empty);
        }
        
        private Patient(string mrnPrefix, RandomPersonGenerator.Result result) : base(result)
        {
            Initialize(mrnPrefix);
        }

        public static List<Patient> GetRandomPatients(int count)
        {
            List<Patient> patients = new List<Patient>();

            foreach (var result in RandomPersonGenerator.GetRandomResult(count).Results)
            {
                Patient patient = new Patient(result);
                patients.Add(patient);
            }

            return patients;
        }

        private void Initialize(string mrnPrefix)
        {
            MRN = MrnGenerator.GetNewMrn(mrnPrefix);
            MPI = MRN;
        }

        private static class MrnGenerator
        {
            private static readonly object _object = new object();
            private static int currentMessageID = new Random().Next(0, 100000);

            public static string GetNewMrn(string mrnPrefix)
            {
                int messageID = 0;

                lock (_object)
                {
                    messageID = currentMessageID++;
                }

                return string.Format("{0}{1}{2}", mrnPrefix, DateTime.Now.ToString("yyMMdd"), messageID.ToString("00000"));
            }
        }
    }
}
