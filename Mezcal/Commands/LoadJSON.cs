using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Mezcal.Connections;

namespace Mezcal.Commands
{
    public class LoadJSON : ICommand
    {
        public void Process(JObject command, Context context)
        {
            // get default property
            var file = JSONUtil.GetText(command, "#load-json");
            if (file == null) { file = JSONUtil.GetText(command, "file"); }

            // get all other properties
            var set = JSONUtil.GetText(command, "set");

            // apply variables to properties (do this when getting property instead?)
            file = context.ReplaceVariables(file);

            // perform the action
            //Console.WriteLine("Loading JSON File {0} into {1}", file, set);
            var sub = JSONUtil.ReadFile(file);
            context.Store(set, sub);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject jCommand = new JObject();
            jCommand.Add("command", "load-json");

            commandEngine.PromptForArgument(jCommand, "file");
            commandEngine.PromptForArgument(jCommand, "set");

            return jCommand;
        }
    }
}
