using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
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

        public ICommand ResolveCommand(JObject joCommand)
        {
            ICommand result = null;

            var commandName = JSONUtil.GetCommandName(joCommand);

            if (commandName == null) { return null; }
            if (commandName == "load-json") { result = new LoadJSON(); }
            else if (commandName == "save-json") { result = new SaveJSON(); }
            else if (commandName == "prompt") { result = new PromptCommand(); }
            else if (commandName == "run-script") { result = new RunScript(); }
            else if (commandName == "communicate") { result = new Communicate(); }
            else if (commandName == "say") { result = new Communicate(); }
            //else if (commandName == "run-rules") { return new RunRules(); }
            else if (commandName == "assert") { return new Assert(); }
            else if (commandName == "store") { return new Store(); }

            return result;
        }
    }
}
