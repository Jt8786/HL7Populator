using HL7Populator.HL7.V2;
using HL7Populator.Logging;
using HL7Populator.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HL7Populator
{
    internal class OutboundSocket
    {
        public event EventHandler<MessageReceivedEventArgs> AckMessageReceived;
        
        private CancellationTokenSource ct = new CancellationTokenSource();

        private TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();


        private int _port { get; set; }
        private string _remoteServer { get; set; }
        private Socket _socket { get; set; }


        public OutboundSocket(string remoteServer, int port)
        {
            _remoteServer = remoteServer;
            _port = port;
            _socket = new Socket(remoteServer, port);

            _socket.MessageReceived += Socket_MessageReceived;
        }

        public async Task<bool> SendMessage(Message message)
        {
            try
            {
                tcs = new TaskCompletionSource<bool>();
                var sendTask = _socket.SendHL7Message(message);
                var receiveTask = tcs.Task;
                await Task.Run(() => Task.WaitAll(sendTask, receiveTask)).ConfigureAwait(true);

                return true;
            }
            catch (NetworkException e)
            {
                Log.CoreLogger.Error("Network Exception sending message to " + _remoteServer + " on port " + _port + "\r\n" + e.ToString());
            }
            catch (Exception e)
            {
                Log.CoreLogger.Error("Exception sending message to " + _remoteServer + " on port " + _port + "\r\n" + e.ToString());
            }

            return false;
        }

        private void Initialize(string remoteServer, int port, int timeout)
        {
            this._remoteServer = remoteServer;
            this._port = port;
            Log.CoreLogger.Info(string.Format("Initializing outbound connection to {0} on port {1} with timeout {2}", remoteServer, port, timeout));
            _socket = new Socket(remoteServer, port);

            _socket.MessageReceived += Socket_MessageReceived;
        }

        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            tcs.SetResult(true);

            Log.CoreLogger.Debug("Client received message on port " + _port);

            AckMessage message = new AckMessage();
            message[ControlCharacters.Default] = e.Message.ToString();

            AckMessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = message });
        }
    }
}
