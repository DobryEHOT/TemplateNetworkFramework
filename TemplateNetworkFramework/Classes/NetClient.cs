using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TemplateNetworkFramework.Classes
{


    public class ClientInfo
    {
        public string Name { get; private set; }
        public string hashPassword { get; private set; } = "";

        public TcpClient ClientTCP { get; private set; }

        public ClientInfo(string name, string password)
        {
            Name = name;
            hashPassword = password;
            ClientTCP = new TcpClient();
        }

        public ClientInfo(string name, string password, TcpClient client) : this(name, password)
        {
            ClientTCP = client;
        }
    }

    public class NetClient
    {
        public ClientInfo info { get; private set; }
        public bool isWork { get; private set; } = false;

        private Sendner sendner;
        private NetworkStream stream;

        private CommandController commandController;

        public NetClient(string name, string password)//, List<TemplateCommand> customCommand)
        {
            var passwordHash = "";

            if (password != null && password != "")
                passwordHash = password.GetHashCode().ToString();
            else
                passwordHash = "";

            info = new ClientInfo(name, passwordHash);
            sendner = new Sendner();
            commandController = new CommandController();//customCommand);
        }

        public NetClient(string name, string password, Action<string> DebugMethod) : this(name, password)
        {
            DebugAdapter.DebugLog = DebugMethod;
        }

        public void Connect(string ip, int port)
        {
            if (isWork)
            {
                DebugAdapter.Log("You already connect");
                return;
            }

            info = new ClientInfo(info.Name, info.hashPassword);
            info.ClientTCP.Connect(ip, port);
            stream = info.ClientTCP.GetStream();
            sendner.Start();
            isWork = true;
            SendCommand<TemplateConnect>(info.Name + " " + info.hashPassword);
            sendner.asynListeningClient(info, allocServerSended, allocToDrop);
        }

        public void Disconnect()
        {
            SendCommand<TemplateDisconnect>(info.Name + " " + info.hashPassword);
        }

        public void SetTickRate(int coutPacks)
        {
            sendner.TickRate = coutPacks;
        }

        private void allocServerSended(ClientInfo info, byte[] buffer)
        {
            var message = commandController.GetDescriptWhithCommands(buffer, null, this);
            DebugAdapter.Log($"Take message: {message.Message}");
        }

        public void SendCommand<T>(string message) where T : TemplateCommand
        {
            if (!isWork)
            {
                allocToDrop(info, new Exception("Client dosen't connect"));
                return;
            }

            if (message == null)
            {
                message = "";
            }

            var buffer = commandController.GetBufferMessage<T>(message);
            if (buffer == null)
            {
                DebugAdapter.Log($"Havan't \"{typeof(T).Name}\" command");
                return;
            }

            var descripte = commandController.GetDescriptMessage(buffer);
            sendner.asynSendMessageFromClient(descripte, info, allocSended, allocToDrop);
        }

        public void SendCommand<T>() where T : TemplateCommand
        {
            SendCommand<T>("");
        }

        public void AbortClient(ClientInfo info)
        {
            try
            {
                info.ClientTCP.Close();
                stream.Close();
                sendner.Stop();
            }
            catch
            { }
            finally
            {
                isWork = false;
                DebugAdapter.Log("Client disconnected");
            }
        }

        private void allocToDrop(ClientInfo info, Exception exception)
        {
            DebugAdapter.Log($"Client drop: {exception}");

        }

        private void allocSended(ClientInfo info, MessageDescript descript)
        {
            DebugAdapter.Log($"Sended: {descript.Message}");
        }


    }

}
