using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes.Tools
{
    public class LoopIterator
    {
        public Queue<InfoMethodLoop> methods { get; private set; }

        public LoopIterator()
        {
            methods = new Queue<InfoMethodLoop>();
        }

        public void AddTask(InfoMethodLoop info)
        {
            methods.Enqueue(info);
        }

        public void Update()
        {
            if (methods.Count > 0)
            {
                methods.Dequeue().DoMethod();
            }
        }
    }

    public class InfoMethodLoop
    {
        private Action<NetServer, NetClient, MessageDescript> action;

        private NetServer server;
        private NetClient client;
        private MessageDescript descript;

        public InfoMethodLoop(NetServer server, NetClient client, MessageDescript descript,
            Action<NetServer, NetClient, MessageDescript> action)
        {
            this.server = server;
            this.client = client;
            this.descript = descript;
            this.action = action;
        }

        public void DoMethod()
        {
            if (action != null)
                action.Invoke(server, client, descript);
        }
    }
}
