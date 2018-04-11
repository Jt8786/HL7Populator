using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7Populator.HL7.V2
{
    public class AckMessageReceivedEventArgs : EventArgs
    {
        private static object _object = new object();
        private static int currentMessageID = 0;

        public int InternalMessageID { get; set; }
        public AckMessage Message { get; set; }

        public AckMessageReceivedEventArgs()
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
