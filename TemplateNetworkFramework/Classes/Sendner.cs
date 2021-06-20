using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TemplateNetworkFramework.Classes.Tools;

namespace TemplateNetworkFramework.Classes
{
    public class Sendner
    {
        public NetworkStream stream { get; set; }

        private QueueThreadProtected<InfoSendRecvest> sendRecvests = new QueueThreadProtected<InfoSendRecvest>();
        private List<Thread> threads = new List<Thread>();
        private bool isWork = false;
        readonly object locker = new object();
        readonly object locker2 = new object();
        private bool sendingNow = false;
        private int tikeRate = 1000;

        public int TickRate
        {
            get
            {
                return tikeRate;
            }
            set
            {
                if (value > 1000)
                    tikeRate = 1;
                else
                    tikeRate = 1000 / value;
            }
        }

        public Sendner()
        {
            TickRate = 1000;
        }

        public void Start()
        {
            isWork = true;
        }

        public void Stop()
        {
            isWork = false;
            foreach (var thr in threads)
            {
                try
                {
                    thr.Abort();
                }
                finally
                { }
            }
            threads = new List<Thread>();
        }

        public void asynSendMessageFromClient(MessageDescript descript, ClientInfo client, Action<ClientInfo, MessageDescript> allocToSendedMessage, Action<ClientInfo, Exception> allocToDropSendClient)
        {
            InfoSendRecvest info = new InfoSendRecvest(descript, client, allocToSendedMessage, allocToDropSendClient);


            sendRecvests.Enqueue(info);

            if (sendingNow)
                return;

            sendingNow = true;
            StartProtectedThread(SendingQueue);
        }

        private void SendMessage(MessageDescript descript, ClientInfo client, Action<ClientInfo, MessageDescript> allocToSendedMessage, Action<ClientInfo, Exception> allocToDropSendClient)
        {
            lock (locker)
            {
                try
                {
                    var stream = client.ClientTCP.GetStream();

                    stream.Write(descript.Buffer, 0, descript.Buffer.Length);

                    allocToSendedMessage(client, descript);
                }
                catch (Exception e)
                {
                    lock (locker)
                    {
                        allocToDropSendClient(client, e);
                    }
                }
            }

        }

        public void asynFindingClient(TcpListener listener, string password, Action<ClientInfo> allocToConnectClient, Action<Exception> allocToCrashed)
        {
            threads.Add(StartProtectedThread(Find));

            void Find()
            {
                try
                {
                    Searching();
                }
                catch (ThreadAbortException e)
                {
                    if (e != null)
                        e = null;
                }
                catch (Exception e)
                {
                    lock (locker)
                    {
                        allocToCrashed(e);
                    }
                }
            }

            void Searching()
            {
                while (isWork)
                {
                    TcpClient clientTCP = listener.AcceptTcpClient();

                    NetworkStream stream = clientTCP.GetStream();
                    stream.ReadTimeout = 1000;

                    byte[] buffer = new byte[40];
                    try
                    {
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        continue;
                    }


                    byte[] clearBuffer = buffer.ToList().GetRange(2, buffer.Length - 2).ToArray();
                    string data = Encoding.UTF8.GetString(clearBuffer);
                    var info = data.Split(' ');

                    if (CheckingOnFalseData(info))
                    {
                        clientTCP.Close();
                        stream.Close();
                        continue;
                    }

                    ClientInfo client = new ClientInfo(info[0], info[1], clientTCP);

                    lock (locker)
                    {
                        allocToConnectClient(client);
                    }
                }

                bool CheckingOnFalseData(string[] data)
                {
                    if (data.Length >= 2 && password != "" && data[1].CompareTo(password) == 0)
                        return false;
                    else if (password == "" && data.Length > 0)
                        return false;
                    else
                        return true;
                }
            }
        }

        public void asynListeningClient(ClientInfo client, Action<ClientInfo, byte[]> allocClientSendMessageFromServer, Action<ClientInfo, Exception> allocToCrashed)
        {
            threads.Add(StartProtectedThread(Listen));

            void Listen()
            {
                try
                {
                    while (isWork)
                        Listening();
                }
                catch (ThreadAbortException e)
                {
                    if (e != null)
                        e = null;
                }
                catch (Exception e)
                {
                    lock (locker)
                    {
                        allocToCrashed(client, e);
                    }
                }
            }

            void Listening()
            {
                NetworkStream stream = client.ClientTCP.GetStream();

                stream.ReadTimeout = Timeout.Infinite;

                List<byte> resutBuffer = new List<byte>();

                byte[] buffer = ReadStreemBuffer(stream, 2, resutBuffer);

                if (buffer[1] > 0)
                    buffer = ReadStreemBuffer(stream, buffer[1], resutBuffer);

                buffer = resutBuffer.ToArray();

                lock (locker)
                {
                    allocClientSendMessageFromServer(client, buffer);
                }

                Thread.Sleep(TickRate);
            }

            byte[] ReadStreemBuffer(NetworkStream stream, int countByte, List<byte> resutBuffer)
            {
                byte[] buffer = new byte[countByte];
                stream.Read(buffer, 0, buffer.Length);

                resutBuffer.AddRange(buffer);
                return buffer;
            }
        }

        private void SendingQueue()
        {

            do
            {

                var info = new InfoSendRecvest(null, null, null, null);
                if (sendRecvests.TryDequeue(out info))
                    SendMessage(info.Descript, info.Client, info.AllocToSendedMessage, info.AllocToDropSendClient);
                else
                    return;

            }
            while (sendRecvests.Count() != 0);


            sendingNow = false;
        }

        private Thread StartProtectedThread(Action action)
        {
            if (!isWork)
                throw new Exception("Sendner dosen't work!");

            Thread threadListners = new Thread(new ThreadStart(action));
            threadListners.Start();

            return threadListners;
        }
    }

    internal class InfoSendRecvest
    {
        public MessageDescript Descript { get; private set; }
        public ClientInfo Client { get; private set; }
        public Action<ClientInfo, MessageDescript> AllocToSendedMessage { get; private set; }
        public Action<ClientInfo, Exception> AllocToDropSendClient { get; private set; }

        public InfoSendRecvest(MessageDescript descript,
            ClientInfo client,
            Action<ClientInfo, MessageDescript> allocToSendedMessage,
            Action<ClientInfo, Exception> allocToDropSendClient)
        {
            Descript = descript;
            Client = client;
            AllocToSendedMessage = allocToSendedMessage;
            AllocToDropSendClient = allocToDropSendClient;
        }
    }
}
