using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.Core
{
    public class Exam
    {
        public Patient Patient { get; set; }
        public string Accession { get; set; }
        public string Site { get; set; }
        public Procedure Procedure { get; set; }

        public Physician Consulting { get; set; }
        public Physician Attending { get; set; }
        public Physician Referring { get; set; }
        public Physician Admitting { get; set; }

        public Exam()
        {
            Initialize(string.Empty, new Patient());
        }

        public Exam(string accessionPrefix)
        {
            Initialize(accessionPrefix, new Patient());
        }

        public Exam(Patient patient)
        {
            Initialize(string.Empty, patient);
        }

        public Exam(Patient patient, string accessionPrefix)
        {
            Initialize(accessionPrefix, patient);
        }

        public static List<Exam> GetRandomExams(int count)
        {
            List<Exam> exams = new List<Exam>();

            foreach (var patient in Patient.GetRandomPatients(count))
            {
                Exam exam = new Exam(patient);
                exams.Add(exam);
            }

            return exams;
        }

        public static List<Exam> GetRandomExamsWithPhysicians(int count)
        {
            var exams = new List<Exam>();
            var physicians = Physician.GetRandomPhysicians(count);
            int i = 0;
            foreach (var patient in Patient.GetRandomPatients(count))
            {
                Exam exam = new Exam(patient);
                exam.Attending = physicians[i];
                exams.Add(exam);
                i++;
            }

            return exams;
        }

        private void Initialize(string accessionPrefix, Patient patient)
        {
            Accession = AccessionGenerator.GetNewAccession(accessionPrefix);
            Procedure = ProcedureGenerator.GetProcedure();
            Patient = patient;
        }

        private static class AccessionGenerator
        {
            private static readonly object _object = new object();
            private static int currentMessageID = new Random().Next(0, 1000);

            public static string GetNewAccession(string accessionPrefix)
            {
                int messageID = 0;

                lock (_object)
                {
                    messageID = currentMessageID++;
                }

                return string.Format("{0}{1}{2}", accessionPrefix, DateTime.Now.ToString("yyMMdd"), messageID.ToString("000"));
            }
        }

        private static class ProcedureGenerator
        {
            private const string defaultProcCode = "XRANKLE2V";
            private const string defaultProcDesc = "XR Ankle 2 Views";

            private static readonly object _object = new object();
            private static HashSet<Procedure> procedures = new HashSet<Procedure>();
            private static Random randomGenerator = null;

            public static Procedure GetProcedure()
            {
                if (procedures.Count == 0)
                    lock (_object)
                    {
                        var lines = new string[0];

                        try
                        {
                            lines = File.ReadAllLines("Procedures.txt");
                        }
                        catch
                        {
                            Logging.Log.CoreLogger.Error("Unable to location Procedures.txt");
                        }

                        if (lines.Count() > 0)
                        {
                            foreach (var line in lines)
                            {
                                Procedure proc = new Procedure();
                                var split = line.Split(new string[] { "-" }, 2, StringSplitOptions.None);
                                if (split.Count() < 2)
                                    continue;
                                proc.Code = split[0];
                                proc.Description = split[1];
                                procedures.Add(proc);
                            }
                        }
                        else
                        {
                            Procedure proc = new Procedure();
                            proc.Code = defaultProcCode;
                            proc.Description = defaultProcDesc;

                            procedures.Add(proc);
                        }
                        randomGenerator = new Random();
                    }

                int item = 0;
                lock (_object)
                {
                    item = randomGenerator.Next(0, procedures.Count - 1);
                }
                
                return procedures.ElementAt(item);
            }
        }
    }
}
