using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateNetworkFramework.Classes.Tools;

namespace TemplateNetworkFramework.Classes
{
    public static class TNFManager
    {
        private static List<TemplateCommand> Commands = new List<TemplateCommand>();
        public static LoopIterator Iterator { get; private set; } = new LoopIterator();

        public static void InitializationCustomCommand(LibraryTemplates commands)
        {
            var com = commands.GetCommandsLibrary();

            if (com != null)
                Commands.AddRange(com);
        }

        public static List<TemplateCommand> GetCustomCommands() => Commands;
       
    }
}
