using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HL7Populator.HL7.V2
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private static object _object = new object();
        private static int currentMessageID = 0;

        public int InternalMessageID { get; set; }
        public Message Message { get; set; }

        public MessageReceivedEventArgs()
        {
            InternalMessageID = GetInternalMessageID();
        }

        private static int GetInternalMessageID()
        {
            lock (_object)
            {
                currentMessageID++;
                return currentMessageID;
            }
        }
    }
}
