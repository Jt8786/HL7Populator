using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL7Populator.HL7.V2;

namespace HL7Populator.Test
{
    [TestClass]
    public class HL7Test
    {
        [TestMethod]
        public void SendMessagesToListener()
        {
            var server = new Server(2575, "localhost", 2575, Logging.Log.LogLevel.All);

            server.SendRandomORUs(10);

            System.Threading.Thread.Sleep(10000);
        }
    }
}
