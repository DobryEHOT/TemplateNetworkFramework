using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes
{
    public class NetServer
    {

        public Dictionary<string, ClientInfo> clients { get; private set; }
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }
        public string serverPassword { get; private set; } = "";

        private TcpListener listner;
        private Sendner sendner;
        private CommandController commandController;


        public bool isWorking { get; private set; } = false;


        public NetServer(IPAddress ip, int port)
        {
            IP = ip;
            Port = port;

            listner = new TcpListener(IP, Port);
            clients = new Dictionary<string, ClientInfo>();
            sendner = new Sendner();
            commandController = new CommandController();// null);
        }

        public NetServer(IPAddress ip, int port, string serverPassword) : this(ip, port)
        {
            if (serverPassword != null && serverPassword != "")
                this.serverPassword = serverPassword.GetHashCode().ToString();
            else
                this.serverPassword = "";
        }

        public NetServer(IPAddress ip, int port, string serverPassword, List<TemplateCommand> customCommand) : this(ip, port, serverPassword)
        {
            commandController = new CommandController();// customCommand);
        }

        public NetServer(IPAddress ip, int port, string serverPassword, List<TemplateCommand> customCommand, Action<string> DebugMethod) : this(ip, port, serverPassword, customCommand)
        {
            DebugAdapter.DebugLog = DebugMethod;
        }

        public void StartServer()
        {
            if (isWorking)
                return;

            isWorking = true;

            DebugAdapter.Log("Server is running");
            DebugAdapter.Log($"Server password: {serverPassword}");


            listner.Start();
            sendner.Start();
            sendner.asynFindingClient(listner, serverPassword, allocFindedClient, allocBreackServer);
        }

        public void StopServer()
        {
            foreach (var clientInfo in clients)
            {
                clientInfo.Value.ClientTCP.GetStream().Close();
                clientInfo.Value.ClientTCP.Close();
            }

            listner.Stop();
            sendner.Stop();
            isWorking = false;

            DebugAdapter.Log("Server is stoped");
        }

        public void SetTickRate(int coutPacks)
        {
            sendner.TickRate = coutPacks;
        }

        public void SendCommandFromAllClients<T>(string message) where T : TemplateCommand
        {
            var buffer = commandController.GetBufferMessage<T>(message);
            if (buffer == null)
                return;

            var descripte = commandController.GetDescriptMessage(buffer);
            SendCommandFromAllClients(descripte);
        }

        public void SendCommandFromAllClients(MessageDescript descript)
        {
            foreach (var client in clients)
            {
                sendner.asynSendMessageFromClient(descript, client.Value, allocSendedMessageFromClient, allocDropClientSendMessage);
            }
        }

        public void DisconnectClient(string name)
        {
            if (clients.ContainsKey(name))
            {
                var info = clients[name].ClientTCP;
                try
                {
                    info.GetStream().Close();
                    info.Close();
                }
                catch { }
                finally
                {
                    clients.Remove(name);
                }
            }
        }

        private void allocFindedClient(ClientInfo client)
        {
            clients.Add(client.Name, client);
            DebugAdapter.Log($"Client \"{client.Name}\" has join to server");

            sendner.asynListeningClient(client, allocSendedMessageClientFromServer, allocDropClientSendMessage);
        }

        private void allocBreackServer(Exception e)
        {
            DebugAdapter.Log("Server is crashed: " + e.Message);
            StopServer();
        }

        private void allocDropClientSendMessage(ClientInfo client, Exception e)
        {
            DebugAdapter.Log("Drop client: " + e);
            DisconnectClient(client.Name);
            SendCommandFromAllClients<TemplateDisconnect>(client.Name);
            DebugAdapter.Log($"Client \"{client.Name}\" has been removed");
        }

        private void allocSendedMessageFromClient(ClientInfo client, MessageDescript descript)
        {

        }

        private void allocSendedMessageClientFromServer(ClientInfo client, byte[] buffer)
        {
            var descript = commandController.GetDescriptWhithCommands(buffer, this, null);
            if (descript == null)
            {
                DebugAdapter.Log($"Server take brake message from {client.Name}");
            }
            else if (descript.Tempate is TemplateDisconnect)
            {
                DisconnectClient(client.Name);
            }
            else
            {
                DebugAdapter.Log($"Server take message from {client.Name}: {descript.Message}");
                SendCommandFromAllClients(descript);
            }
        }
    }
}
