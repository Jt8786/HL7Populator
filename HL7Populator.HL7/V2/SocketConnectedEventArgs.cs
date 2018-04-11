using System;

namespace HL7Populator.HL7.V2
{
    public class SocketConnectedEventArgs : EventArgs
    {
        public Socket Socket { get; set; }
    }
}