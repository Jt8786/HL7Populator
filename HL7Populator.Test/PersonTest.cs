using HL7Populator.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.Test
{
    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void CreatePatient()
        {
            var patient = new Patient();

            Debug.WriteLine("Patient: {0} {1} MRN: {2}", patient.FirstName, patient.LastName, patient.MRN);
        }
    }
}
