namespace HL7Populator.Network
{
    using System;

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public double ConnectionID { get; set; }
    }
}