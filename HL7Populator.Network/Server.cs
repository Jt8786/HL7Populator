namespace HL7Populator.Network
{
    using HL7Populator.Logging;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class Server
    {
        private CancellationTokenSource ct = new CancellationTokenSource();

        private TcpListener server { get; set; }
        private int port { get; set; }
        private int timeOut { get; set; }

        public event EventHandler ClientConnected;

        public Server(int port, int timeout)
        {
            this.port = port;
            timeOut = timeout;
            InitializeServer();
        }

        public async Task StartListening()
        {
            Log.NetworkLogger.Info(string.Format("Starting to listen on TCP port {0}", port));

            try
            {
                ct = new CancellationTokenSource();
                server.Start();
                // Fire and forget
                await AcceptClientsAsync(server);
            }
            catch (Exception e)
            {
                Log.NetworkLogger.Error(string.Format("Error starting listener on port {0}:\r\n{1}", port, e.ToString()));
                throw new NetworkException(e.ToString());
            }
        }

        public void StopListening()
        {
            ct.Cancel();
            server.Stop();
        }

        async Task AcceptClientsAsync(TcpListener listener)
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync()
                                                    .ConfigureAwait(false);
                var networkClient = new Client(client, timeOut);
                Log.NetworkLogger.Info("Client {0} connected from IP {1} to port {2}", networkClient.ConnectionID, ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), port);
                ClientConnected?.Invoke(this, new ClientConnectedEventArgs() { Client = networkClient });
            }

        }

        private void InitializeServer()
        {
            server = new TcpListener(IPAddress.Any, port);
        }
    }
}
