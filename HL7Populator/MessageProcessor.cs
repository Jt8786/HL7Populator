namespace HL7Populator
{
    public abstract class MessageProcessor
    {
        /// <summary>
        /// Abstract class used to handle messages by an outside library
        /// </summary>
        /// <param name="server">Server object it will process messages for</param>
        protected MessageProcessor(Server server)
        {
            server.MessageEvent.MessageProcessor = this;
        }

        /// <summary>
        /// Override this method to accept and process all received messages
        /// </summary>
        /// <param name="message">Received message to be processed</param>
        public abstract void ProcessReceivedMessage(HL7.V2.Message message);

        /// <summary>
        /// Before a message is sent, you can manually add your own logic to adjust the message as you see fit
        /// </summary>
        /// <param name="message">HL7 message to be sent</param>
        /// <returns></returns>
        public abstract HL7.V2.Message InterceptMessage(HL7.V2.Message message);
    }
}