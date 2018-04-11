using HL7Populator.HL7.V2;

namespace HL7Populator
{
    internal class MessageEvent
    {
        public MessageProcessor MessageProcessor { get; set; } 

        public void Notify(Message message)
        {
            if (null != MessageProcessor)
                MessageProcessor.ProcessReceivedMessage(message);
        }

        public Message Intercept(Message message)
        {
            if (null != MessageProcessor)
                return MessageProcessor.InterceptMessage(message);

            return message;
        }
    }
}
