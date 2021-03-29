using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes
{
    public static class DebugAdapter
    {
        public static Action<string> DebugLog;

        public static void Log(string message)
        {
            if (DebugLog == null)
                return;

            DebugLog(message);
        }
    }
}
