using HL7Populator.Core;
using System.Threading;
using System.Threading.Tasks;

namespace HL7Populator
{
    public class Server
    {
        private InboundSocket inboundSocket { get; set; }
        private OutboundSocket outboundSocket { get; set; }

        internal MessageEvent MessageEvent { get; private set; }

        /// <summary>
        /// Creates an instance of the HL7 server to send and receive HL7 messages
        /// </summary>
        /// <param name="inboundPort">Port to listen for inbound messages on. Pass 0 to not listen</param>
        /// <param name="outboundAddress">Remote server that accepts the HL7 messages. Pass an empty string to not create a client</param>
        /// <param name="outboundPort">Remote server TCP port to accept. Pass 0 to not create a client</param>
        /// <param name="logLevel">log4net logging level. Info is default, which outputs the HL7 traffic to C:\ProgramData\HL7Populator</param>
        public Server(int inboundPort, string outboundAddress, int outboundPort, Logging.Log.LogLevel logLevel = Logging.Log.LogLevel.Info)
        {
            Logging.Log.SetLoggingLevel(logLevel);

            if (inboundPort != 0)
            {
                inboundSocket = new InboundSocket(inboundPort, 0);
                inboundSocket.MessageReceived += MessageReceived;
            }

            if (!string.IsNullOrEmpty(outboundAddress) && outboundPort != 0)
            {
                outboundSocket = new OutboundSocket(outboundAddress, outboundPort);
                outboundSocket.AckMessageReceived += MessageReceived;
            }

            MessageEvent = new MessageEvent();
        }

        /// <summary>
        /// Stop the inbound socket
        /// </summary>
        public void Stop()
        {
            if (inboundSocket != null)
                inboundSocket.Stop();
        }

        /// <summary>
        /// Send the specified number of ADT-A08s to the configured outbound server
        /// </summary>
        /// <param name="count">Number of ADTs to send</param>
        public void SendRandomADTs(int count)
        {
            Task.Run(() => SendADTs(count)).ConfigureAwait(true);
        }

        /// <summary>
        /// Send the specified number of ORMs to the configured outbound server
        /// </summary>
        /// <param name="count">Number of ORMs to send</param>
        public void SendRandomORMs(int count)
        {
            Task.Run(() => SendORMs(count)).ConfigureAwait(true);
        }

        /// <summary>
        /// Send the specified number of ORUs to the configured outbound server
        /// </summary>
        /// <param name="count">Number of ORUs to send</param>
        public void SendRandomORUs(int count)
        {
            Task.Run(() => SendORUs(count)).ConfigureAwait(true);
        }

        /// <summary>
        /// Send an HL7 message to the configured outbound server
        /// </summary>
        /// <param name="message">HL7 message to send</param>
        public void SendMessage(HL7.V2.Message message)
        {
            Task.Run(() => SendMessageAsync(message));
        }

        /// <summary>
        /// Send an HL7 message to the configured outbound server asynchronously
        /// </summary>
        /// <param name="message">HL7 message to send</param>
        public async Task SendMessageAsync(HL7.V2.Message message)
        {
            var update = MessageEvent.Intercept(message);

            await outboundSocket.SendMessage(update).ConfigureAwait(true);
        }

        private async Task SendADTs(int count)
        {
            var patients = Patient.GetRandomPatients(count);

            foreach (var patient in patients)
            {
                await SendMessageAsync(MessageGenerator.GetMessageFromPatient(patient)).ConfigureAwait(true);
            }
        }

        private async Task SendORMs(int count)
        {
            var exams = Exam.GetRandomExams(count);

            foreach (var exam in exams)
            {
                await SendMessageAsync(MessageGenerator.GetMessageFromExam(exam)).ConfigureAwait(true);
            }
        }

        private async Task SendORUs(int count)
        {
            var exams = Exam.GetRandomExamsWithPhysicians(count);

            foreach (var exam in exams)
            {
                await SendMessageAsync(MessageGenerator.GetResultMessageFromExam(exam)).ConfigureAwait(true);
            }
        }

        private void MessageReceived(object sender, HL7.V2.MessageReceivedEventArgs e)
        {
            MessageEvent.Notify(e.Message);
        }
    }
}
