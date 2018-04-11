using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HL7Populator.HL7.V2
{
    public class Listener
    {
        private readonly object _socketsObject = new object();
        private readonly object _acksObject = new object();
        private Network.Server server { get; set; }
        private Dictionary<int, int> pendingAcks { get; set; }

        public List<Socket> Sockets { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Listener(int port)
        {
            Initialize(port, 0);
        }

        public Listener(int port, int timeout)
        {
            Initialize(port, timeout);
        }

        public async Task SendAck(int internalMessageID, Message message)
        {
            Socket socket = GetSocketByInternalMessageID(internalMessageID);

            await socket.SendHL7Message(message).ConfigureAwait(true);

            lock (_acksObject)
            {
                pendingAcks.Remove(internalMessageID);
            }
        }

        private Socket GetSocketByInternalMessageID(int internalMessageID)
        {
            if (!pendingAcks.ContainsKey(internalMessageID))
                throw new HL7Exception("No socket pending ACK with internal message ID " + internalMessageID);

            var socket = Sockets.Where(t => t.ConnectionID == pendingAcks[internalMessageID]).FirstOrDefault();
            if (null == socket)
                throw new Network.NetworkException("Socket no longer connected");
            
            return socket;
        }

        public void StopListening()
        {
            server.StopListening();
        }

        private void Initialize(int port, int timeout)
        {
            pendingAcks = new Dictionary<int, int>();

            Sockets = new List<Socket>();
            server = new Network.Server(port, timeout);
            server.ClientConnected += Server_ClientConnected;
            Task.Run(server.StartListening).ConfigureAwait(true);
        }

        private void Server_ClientConnected(object sender, EventArgs e)
        {
            Socket socket = new Socket(((Network.ClientConnectedEventArgs)e).Client);

            lock(_socketsObject)
            {
                Sockets.Add(socket);

                socket.MessageReceived += Socket_MessageReceived;
                socket.SocketDisconnected += Socket_SocketDisconnected;

            }
        }

        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            lock (_acksObject)
            {
                pendingAcks.Add(e.InternalMessageID, ((Socket)sender).ConnectionID);
            }
            MessageReceived?.Invoke(sender, e);
        }

        private void Socket_SocketDisconnected(object sender, EventArgs e)
        {
            var socket = Sockets.Where(t => t.ConnectionID == ((SocketDisconnectedEventArgs)e).ConnectionID).FirstOrDefault();

            if (null != socket)
            {
                socket.MessageReceived -= Socket_MessageReceived;
                socket.SocketDisconnected -= Socket_SocketDisconnected;

                lock(_socketsObject)
                {
                    Sockets.RemoveAll(t => t.ConnectionID == ((SocketDisconnectedEventArgs)e).ConnectionID);
                }
            }
        }
    }
}
