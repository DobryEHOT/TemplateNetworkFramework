using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes
{
    public class CommandController
    {
        private DefaultCommandsList commandsList;
        private Descriptor descriptor;

        public CommandController()//List<TemplateCommand> templates)
        {
            commandsList = new DefaultCommandsList();
            descriptor = new Descriptor(commandsList);

            //if (templates == null)
            //    return;

            //foreach (var element in templates)
            //    commandsList.TryAddCommand(element, (byte)((int)commandsList.GetMaxByte() + 1));
        }

        public byte[] GetBufferMessage<T>(string message) where T : TemplateCommand
        {
            return descriptor.ScriptMessage<T>(message);
        }

        public MessageDescript GetDescriptMessage(byte[] buffer)
        {
            return descriptor.DescriptMessage(buffer);
        }

        public MessageDescript GetDescriptWhithCommands(byte[] buffer, NetServer server, NetClient client)
        {
            var descript = descriptor.DescriptMessage(buffer);
            if (descript == null)
                return null;

            descript.Tempate.GetCommandTemplate(server, client, descript);
            return descript;
        }
    }

    public class DefaultCommandsList
    {
        protected virtual Dictionary<TemplateCommand, byte> commands { get; set; }

        public DefaultCommandsList()
        {
            commands = new Dictionary<TemplateCommand, byte>();

            TryAddCommand(new TemplateDisconnect(), 0);
            TryAddCommand(new TemplateConnect(), 1);
            TryAddCommand(new TemplateStringMessage(), 2);

            var customCommands = TNFManager.GetCustomCommands();
            if (customCommands != null)
                foreach (var command in customCommands)
                {
                    TryAddCommand(command, (byte)((int)GetMaxByte() + 1));
                }

        }

        public bool TryAddCommand(TemplateCommand templateCommand, byte indexCommand)
        {
            if (commands.ContainsKey(templateCommand) || commands.ContainsValue(indexCommand))
                return false;

            commands.Add(templateCommand, indexCommand);

            return true;
        }

        public byte GetMaxByte()
        {
            byte bigger = 0;

            foreach (var value in commands)
                bigger = Math.Max(value.Value, bigger);

            return bigger;
        }

        public byte? GetCommandByte<T>() where T : TemplateCommand
        {
            foreach (var value in commands)
                if (value.Key is T)
                    return value.Value;

            return null;
        }

        public TemplateCommand GetTemplateCommand(byte commandByte)
        {
            if (commands.ContainsValue(commandByte))
                foreach (var value in commands)
                    if (value.Value == commandByte)
                        return value.Key;

            return null;
        }
    }

    internal class Descriptor
    {
        private DefaultCommandsList commandList;

        public Descriptor(DefaultCommandsList list)
        {
            commandList = list;
        }

        public MessageDescript DescriptMessage(byte[] buffer)
        {
            var nameCommand = commandList.GetTemplateCommand(buffer[0]);

            if (nameCommand == null)
                return null;

            int offset = 2;
            byte[] clearBuffer = buffer.ToList().GetRange(offset, buffer.Length - offset).ToArray();
            string message = Encoding.UTF8.GetString(clearBuffer);

            byte cleanByte = buffer[0];
            byte sizeBuf = buffer[1];

            return new MessageDescript(nameCommand, cleanByte, message, buffer, sizeBuf);
        }

        public byte[] ScriptMessage<T>(string message) where T : TemplateCommand
        {
            var byteCom = commandList.GetCommandByte<T>();

            if (byteCom == null)
            {
                return null;
                //throw new Exception($"You need set {typeof(T).Name} in command list");
            }

            List<byte> listBuffer = new List<byte>();
            listBuffer.Add((byte)byteCom);

            byte[] buffer = Encoding.UTF8.GetBytes(message);

            listBuffer.Add((byte)buffer.Length);
            listBuffer.AddRange(buffer);

            return listBuffer.ToArray();
        }
    }

    public sealed class MessageDescript
    {
        public TemplateCommand Tempate { get; private set; }
        public byte CommandByte { get; private set; }
        public string Message { get; private set; }
        public byte[] Buffer { get; private set; }
        public byte BufferSize { get; private set; }

        public MessageDescript(TemplateCommand commandTemplate, byte commandByte, string message, byte[] buffer, byte bufferSize)
        {
            Tempate = commandTemplate;
            CommandByte = commandByte;
            Message = message;
            Buffer = buffer;
            BufferSize = bufferSize;
        }
    }
}
