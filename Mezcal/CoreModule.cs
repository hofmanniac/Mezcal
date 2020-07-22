using Mezcal.Commands;
using Mezcal.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal
{
    internal class CoreModule : IModule
    {
        public IConnection ProvideConnection(ConnectionConfig envConfig)
        {
            return null;
        }

        public ICommand ResolveCommand(string commandName)
        {
            ICommand result = null;

            if (commandName == null) { return null; }
            if (commandName == "load-json") { result = new LoadJSON(); }
            else if (commandName == "save-json") { result = new SaveJSON(); }
            else if (commandName == "ui-prompt") { result = new UIPrompt(); }
            else if (commandName == "run-script") { result = new RunScript(); }

            return result;
        }
    }
}
