namespace HL7Populator.Network
{
    using System;

    public class ClientConnectedEventArgs : EventArgs
    {
        public Client Client { get; set; }
    }
}