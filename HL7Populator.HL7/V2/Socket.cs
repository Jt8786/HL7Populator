using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HL7Populator.HL7.V2
{
    public class Socket
    {
        private readonly object _object = new object();
        private Network.Client client { get; set; }
        private List<byte> receivedBytes { get; set; }

        private string _remoteIPAddress { get; set; }
        private int _port { get; set; }

        internal int ConnectionID
        {
            get
            {
                return client.ConnectionID;
            }
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<SocketDisconnectedEventArgs> SocketDisconnected;

        public Socket(string remoteIpAddress, int port)
        {
            client = new Network.Client(remoteIpAddress, port, 0);
            // Add event handler before start listening
            Initialize();
            client.Start();
        }

        public Socket(string remoteIpAddress, int port, int timeout)
        {
            client = new Network.Client(remoteIpAddress, port, timeout);
            // Add event handler before start listening
            Initialize();
            client.Start();
        }

        internal Socket(Network.Client client)
        {
            this.client = client;
            // Add event handler before start listening
            Initialize();
            client.Start();
        }

        public async Task SendHL7Message(Message message)
        {
            List<byte> byteMessage = new List<byte>();
            var str = message[message.ControlCharacters];
            byteMessage.AddRange(Encoding.ASCII.GetBytes(message[message.ControlCharacters]));

            await client.SendDataAsync(byteMessage.ToArray()).ConfigureAwait(true);
        }

        private void Initialize()
        {
            receivedBytes = new List<byte>();
            client.DataReceived += Client_DataReceived;
            client.ClientDisconnected += Client_ClientDisconnected;
        }

        private void Client_DataReceived(object sender, EventArgs e)
        {
            Logging.Log.HL7Logger.Debug("Enter network client data received");
            lock (_object)
            {
                receivedBytes.AddRange(((Network.DataReceivedEventArgs)e).ReceivedBytes);

                if (receivedBytes.Contains(Convert.ToByte(ControlCharacters.Header)) 
                    && receivedBytes.Contains(Convert.ToByte(ControlCharacters.EndOfMessage)))
                {
                    int start = receivedBytes.LastIndexOf(Convert.ToByte(ControlCharacters.Header));
                    int end = receivedBytes.LastIndexOf(Convert.ToByte(ControlCharacters.EndOfMessage));

                    // remove the start character
                    string hl7string = Encoding.ASCII.GetString(receivedBytes.GetRange(start + 1, end - start -1).ToArray());

                    Message message = new Message(hl7string);

                    receivedBytes = new List<byte>();

                    Logging.Log.HL7Logger.Info("Received HL7 message:\r\n" + message[message.ControlCharacters].Replace("\r", "\r\n").Replace(ControlCharacters.EndOfMessage, ' ').Trim());

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = message });
                }
            }
        }

        private void Client_ClientDisconnected(object sender, EventArgs e)
        {
            SocketDisconnected?.Invoke(this, new SocketDisconnectedEventArgs() { ConnectionID = ConnectionID });
        }
    }
}
