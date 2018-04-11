using HL7Populator.Core;
using HL7Populator.HL7.V2;
using System;
using System.Collections.Generic;

namespace HL7Populator
{
    internal static class MessageGenerator
    {
        private static readonly object _object = new object();
        private static int currentMessageID = new Random().Next();

        public static Message GetMessageFromExam(Exam exam)
        {
            var message = new Message();

            message.Segments.Add(GetMSHSegment(exam.Site, "ORM^O01"));
            message.Segments.AddRange(GetPatientAndVisitSegments(exam.Patient));
            message.Segments.AddRange(GetOrderSegments(exam));

            return message;
        }

        public static Message GetMessageFromPatient(Patient patient)
        {
            var message = new Message();

            message.Segments.Add(GetMSHSegment(string.Empty, "ADT^A08"));
            message.Segments.AddRange(GetPatientAndVisitSegments(patient));

            return message;
        }

        public static Message GetResultMessageFromExam(Exam exam)
        {
            var message = new Message();

            message.Segments.Add(GetMSHSegment(exam.Site, "ORU^O01"));
            message.Segments.AddRange(GetPatientAndVisitSegments(exam.Patient));
            try
            {
                message.Segments.AddRange(GetReportSegments(exam));
            }
            catch(Exception e)
            {
                Logging.Log.HL7Logger.Error("Error creating result message " + e.ToString());
            }

            return message;
        }

        public static AckMessage GetAckMessage(string messageID, AckType ackType, string error = "")
        {
            AckMessage message = new AckMessage();

            message.Segments.Add(GetMSHSegment(string.Empty, "ACK"));
            message.Segments.Add(GetMSASegment(messageID, error, ackType));

            return message;
        }

        private static Segment GetMSASegment(string messageID, string error, AckType ackType)
        {
            Segment MSH = new Segment();

            MSH[new ControlCharacters()] = string.Format("MSA|{0}|{1}|{2}", ackType, messageID, error);

            return MSH;
        }

        private static Segment GetMSHSegment(string site, string messageType)
        {
            Segment MSH = new Segment();

            MSH[new ControlCharacters()] = string.Format("MSH|^~\\&||{0}|||{1}||{2}|{3}|P|2.3", site, DateTime.Now.GetDateTimeString(true), messageType, GetNextMessageID());

            return MSH;
        }

        private static int GetNextMessageID()
        {
            int messageID = 0;

            lock (_object)
            {
                messageID = currentMessageID++;
            }

            return messageID;
        }

        private static List<Segment> GetPatientAndVisitSegments(Patient patient)
        {
            List<Segment> segments = new List<Segment>();

            Segment PID = new Segment();
            PID[new ControlCharacters()] = string.Format("PID|1|{0}|{1}|{2}|{3}^{4}||{5}|{6}|||{7}^^{8}^{9}^{10}||{11}|{12}|||||",
                patient.MPI,
                patient.MRN,
                patient.DeptNumber,
                patient.LastName,
                patient.FirstName,
                patient.DateOfBirth.GetDateTimeString(false),
                patient.Sex.Substring(0, 1),
                patient.Address,
                patient.City,
                patient.State,
                patient.ZipCode,
                patient.Phone,
                patient.Cell);
            segments.Add(PID);

            Segment PIV = new Segment();
            PIV[new ControlCharacters()] = string.Format("PV1|||||||||||||||||||||||||||||||||||||||||||||");
            segments.Add(PIV);

            return segments;
        }
        private static List<Segment> GetOBXSegment(Exam exam, string reportStatus)
        {
            List<Segment> segments = new List<Segment>();

            int lineCount = 1;
            Segment OBX = new Segment();

            OBX[new ControlCharacters()] = string.Format("OBX|{0}|TX|{1}&BODY^{2}||{3}||||||{4}|||{5}|||",
                lineCount,
                exam.Procedure.Code,
                exam.Procedure.Description,
                "Example result text",
                reportStatus,
                DateTime.Now.GetDateTimeString(true));

            segments.Add(OBX);

            return segments;
        }

        private static List<Segment> GetOrderSegments(Exam exam)
        {
            List<Segment> segments = new List<Segment>();

            Segment ORC = new Segment();
            ORC[new ControlCharacters()] = string.Format("ORC|SC||||CM");
            segments.Add(ORC);

            Segment OBR = new Segment();
            OBR[new ControlCharacters()] = string.Format("OBR|{0}||{1}|{2}^{3}||||{4}|||||||||||||||||||^^^{5}|||||||||{6}",
                1,
                exam.Accession,
                exam.Procedure.Code,
                exam.Procedure.Description,
                DateTime.Now.GetDateTimeString(true),
                DateTime.Now.AddMinutes(-5).GetDateTimeString(true),
                DateTime.Now.AddMinutes(-10).GetDateTimeString(true));
            segments.Add(OBR);

            return segments;
        }
        private static List<Segment> GetReportSegments(Exam exam)
        {
            List<Segment> segments = new List<Segment>();

            string reportStatus = "F";

            Segment ORC = new Segment();
                ORC[new ControlCharacters()] = string.Format("ORC|RE||||");
                segments.Add(ORC);

                Segment OBR = new Segment();
                OBR[new ControlCharacters()] = string.Format("OBR|1||{0}|{1}^{2}||||{3}||||||||||||||{3}|||{4}||^^^{3}|||||{5}|{6}|||{3}",
                    exam.Accession,
                    exam.Procedure.Code,
                    exam.Procedure.Description,
                    DateTime.Now.GetDateTimeString(true),
                    reportStatus,
                    exam.Attending != null ? string.Format("{0}&{1}&{2}^^{3}", exam.Attending.HospitalID, exam.Attending.LastName, exam.Attending.FirstName, DateTime.Now.GetDateTimeString(true)) : string.Empty,
                    exam.Consulting != null ? string.Format("{0}&{1}&{2}", exam.Attending.HospitalID, exam.Attending.LastName, exam.Attending.FirstName) : string.Empty);
                segments.Add(OBR);

            segments.AddRange(GetOBXSegment(exam, reportStatus));

            return segments;
        }
    }
}
