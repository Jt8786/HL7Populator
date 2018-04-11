namespace HL7Populator.Network
{
    using HL7Populator.Logging;
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Client
    {
        private static object _object = new object();
        private static int currentConnectionID = 0;

        private CancellationTokenSource _ct = new CancellationTokenSource();

        private string ipAddress { get; set; }
        private int port { get; set; }
        private int timeOut { get; set; }

        private TcpClient _client { get; set; }

        public int ConnectionID { get; set; }

        public event EventHandler DataReceived;
        public event EventHandler ClientDisconnected;

        internal Client(TcpClient client, int timeout)
        {
            Initialize(client, timeout);
        }

        public Client(string ipAddress, int port, int timeout)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            Initialize(new TcpClient(ipAddress, port), timeout);
        }

        public void Start()
        {
            Log.NetworkLogger.Debug("Starting client");
            if (timeOut == 0)
                Task.Run(ReceiveMessageAsync);
            else
                Task.Run(ReceiveMessageAsyncWithTimeout);
        }

        public void Stop()
        {
            _ct.Cancel();
        }

        public async Task SendDataAsync(byte[] buf)
        {
            try
            {
                if (null == _client)
                    _client = new TcpClient(ipAddress, port);

                if (!_client.Connected)
                    if (!string.IsNullOrEmpty(ipAddress) && port != 0)
                        await _client.ConnectAsync(ipAddress, port);
                    else
                        throw new Exception("Client is no longer connected");

                var stream = _client.GetStream();
                await stream.WriteAsync(buf, 0, buf.Length, _ct.Token)
                    .ConfigureAwait(false);
                Log.NetworkLogger.Debug(string.Format("Message sent"));
            }
            catch (Exception e)
            {
                Log.NetworkLogger.Error(string.Format("Error sending data async to {0}:\r\n{1}", port, e.ToString()));
                throw new NetworkException(e.ToString());
            }
        }

        private static int GetConnectionID()
        {
            int id = 0;

            lock (_object)
            {
                currentConnectionID++;
                id = currentConnectionID;
            }

            return id;
        }

        private async Task ReceiveMessageAsync()
        {
            using (_client)
            {
                Log.NetworkLogger.Debug(string.Format("Start receiving messages async"));
                var buf = new byte[4096];
                using (var stream = _client.GetStream())
                {
                    Log.NetworkLogger.Debug(string.Format("Obtained client stream"));
                    while (!_ct.IsCancellationRequested)
                    {
                        var amountReadTask = stream.ReadAsync(buf, 0, buf.Length, _ct.Token);
                        var amountRead = await amountReadTask.ConfigureAwait(false);

                        if (amountReadTask.Status == TaskStatus.Faulted)
                            break;
                        if (amountRead == 0)
                        {
                            Log.NetworkLogger.Info(string.Format("Client {0} disconnected", ConnectionID));
                            break; //end of stream.
                        }

                        Log.NetworkLogger.Debug(string.Format("Client {0} sent {1} bytes of data", ConnectionID, amountRead));

                        DataReceived?.Invoke(this, new DataReceivedEventArgs() { ReceivedBytes = buf.SubArray(0, amountRead) });
                    }
                }
            }

            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs() { ConnectionID = ConnectionID });
        }

        private async Task ReceiveMessageAsyncWithTimeout()
        {
            using (_client)
            {
                Log.NetworkLogger.Debug(string.Format("Start receiving messages async"));
                var buf = new byte[4096];
                using (var stream = _client.GetStream())
                {
                    while (!_ct.IsCancellationRequested)
                    {
                        var timeoutTask = Task.Delay(timeOut);
                        var amountReadTask = stream.ReadAsync(buf, 0, buf.Length, _ct.Token);
                        var completedTask = await Task.WhenAny(timeoutTask, amountReadTask)
                                                      .ConfigureAwait(false);
                        if (completedTask == timeoutTask)
                        {
                            Log.NetworkLogger.Debug(string.Format("Timeout reached"));
                            
                            break;
                        }
                        else
                        {
                            if (amountReadTask.Status == TaskStatus.Faulted)
                                break;
                            var amountRead = amountReadTask.Result;
                            if (amountRead == 0)
                            {
                                Log.NetworkLogger.Info(string.Format("Client {0} disconnected", ConnectionID));
                                break; //end of stream.
                            }

                            Log.NetworkLogger.Debug(string.Format("Client {0} sent {1} bytes of data: {2}", ConnectionID, amountRead, Encoding.ASCII.GetString(buf, 0, amountRead)));

                            DataReceived?.Invoke(this, new DataReceivedEventArgs() { ReceivedBytes = buf.SubArray(0, amountRead) });
                        }
                    }
                }
            }

            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs() { ConnectionID = ConnectionID });
        }

        private void Initialize(TcpClient client, int timeout)
        {
            _client = client;
            _ct = new CancellationTokenSource();
            timeOut = timeout;
            ConnectionID = GetConnectionID();
        }
    }
}
