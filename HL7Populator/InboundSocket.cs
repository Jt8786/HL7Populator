using HL7Populator.HL7.V2;
using HL7Populator.Logging;
using HL7Populator.Network;
using System;
using System.Threading.Tasks;

namespace HL7Populator
{
    internal class InboundSocket
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private Listener listener { get; set; }
        private int _port { get; set; }

        public InboundSocket(int port, int timeout)
        {
            Initialize(port, timeout);
        }

        public void Stop()
        {
            listener.StopListening();
        }

        private void Initialize(int port, int timeout)
        {
            _port = port;

            Log.CoreLogger.Debug("Initializing port " + port + " with timeout " + timeout);
            listener = new Listener(port, timeout);

            listener.MessageReceived += Listener_MessageReceived;
        }

        private async void Listener_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Log.CoreLogger.Debug("Listener received message on port " + _port);
            MessageReceived?.Invoke(this, e);
            await ProcessMessage(e.InternalMessageID, e.Message);
        }

        private async Task ProcessMessage(int internalMessageID, Message message)
        {
            Log.CoreLogger.Debug("Generating ACK for message received on port " + _port);
            AckMessage ackMessage = MessageGenerator.GetAckMessage(message.MessageID, AckType.AA);

            await SendAck(internalMessageID, ackMessage).ConfigureAwait(false);

            Log.CoreLogger.Debug("ACK sent for message received on port " + _port);
            // send message upstream
        }

        private async Task<bool> SendAck(int internalMessageID, AckMessage ack)
        {
            try
            {
                await listener.SendAck(internalMessageID, ack).ConfigureAwait(true);
                return true;
            }
            catch (NetworkException e)
            {
                Log.CoreLogger.Debug("Network Exception sending ACK for message received on port " + _port + "\r\n" + e.ToString());
            }
            catch (Exception e)
            {
                Log.CoreLogger.Debug("Exception sending ACK for message received on port " + _port + "\r\n" + e.ToString());
                //await listener.SendAck(internalMessageID, MessageGenerator.GetAckMessage(ack.AckMessageID, AckType.AE, e.Message));
            }

            return false;
        }
    }
}
