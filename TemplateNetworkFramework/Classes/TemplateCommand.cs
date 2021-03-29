using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateNetworkFramework.Classes.Tools;

namespace TemplateNetworkFramework.Classes
{
    public abstract class TemplateCommand
    {
        protected abstract bool isWorkInMainThread { get; set; }

        public TemplateCommand()
        { }

        public void GetCommandTemplate(NetServer server, NetClient client, MessageDescript descript)
        {
            if (isWorkInMainThread)
            {
                var info = new InfoMethodLoop(server, client, descript, DoTemplate);
                TNFManager.Iterator.AddTask(info);
            }
            else
            {
                DoTemplate(server, client, descript);
            }
        }

        private void DoTemplate(NetServer server, NetClient client, MessageDescript descript)
        {
            if (client != null)
                OnGetCommandClientLogic(client, descript.Message);

            if (server != null)
                OnGetCommandServerLogic(server, descript.Message);
        }

        protected abstract void OnGetCommandServerLogic(NetServer server, string message);
        protected abstract void OnGetCommandClientLogic(NetClient client, string message);
    }

    public class TemplateConnect : TemplateCommand
    {
        protected override bool isWorkInMainThread { get; set; }

        protected override void OnGetCommandClientLogic(NetClient client, string message)
        {
        }


        protected override void OnGetCommandServerLogic(NetServer server, string message)
        {
        }
    }

    public class TemplateDisconnect : TemplateCommand
    {
        protected override bool isWorkInMainThread { get; set; }

        protected override void OnGetCommandClientLogic(NetClient client, string message)
        {
            //DebugAdapter.Log("TemplateDisconnect DO");
            client.AbortClient();
        }

        protected override void OnGetCommandServerLogic(NetServer server, string message)
        {

        }
    }

    public class TemplateStringMessage : TemplateCommand
    {
        protected override bool isWorkInMainThread { get; set; }

        protected override void OnGetCommandClientLogic(NetClient client, string message)
        {
        }


        protected override void OnGetCommandServerLogic(NetServer server, string message)
        {
        }
    }
}
