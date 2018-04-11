using System;
using System.Runtime.Serialization;

namespace HL7Populator.Logging
{
    [Serializable]
    internal class HL7PopulatorException : Exception
    {
        public HL7PopulatorException()
        {
        }

        public HL7PopulatorException(string message) : base(message)
        {
        }

        public HL7PopulatorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HL7PopulatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}