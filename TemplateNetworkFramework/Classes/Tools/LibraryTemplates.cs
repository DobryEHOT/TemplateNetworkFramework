using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateNetworkFramework.Classes.Tools
{
    public class LibraryTemplates
    {
        protected List<TemplateCommand> commands = new List<TemplateCommand>();

        public LibraryTemplates(params TemplateCommand[] command)
        {
            if (command != null)
                commands.AddRange(command);
        }

        public List<TemplateCommand> GetCommandsLibrary() => commands;
        
    }
}
