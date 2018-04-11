using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.HL7
{
    public class HL7Exception : Exception
    {
        public HL7Exception(string message) : base(message)
        {
        }

        public HL7Exception(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
