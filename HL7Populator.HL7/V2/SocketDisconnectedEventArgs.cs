using System;

namespace HL7Populator.HL7.V2
{
    public class SocketDisconnectedEventArgs : EventArgs
    {
        public int ConnectionID { get; set; }
    }
}