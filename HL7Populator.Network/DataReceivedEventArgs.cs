namespace HL7Populator.Network
{
    using System;

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] ReceivedBytes { get; set; }
    }
}