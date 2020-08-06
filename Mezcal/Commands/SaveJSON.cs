using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public class SaveJSON : ICommand
    {
        public void Process(JObject command, Context context)
        {
            var file = JSONUtil.GetText(command, "#save-json");
            if (file == null) { file = JSONUtil.GetText(command, "file"); }

            string set = CommandEngine.GetCommandArgument(command, "set");
            string mode = JSONUtil.GetText(command, "mode");
            var rawItem = command["item"];

            file = context.ReplaceVariables(file);

            JToken item = null;
            if (set != null) { item = (JToken)context.Fetch(set); }
            else if (rawItem != null) { item = rawItem; }

            //Console.WriteLine("Saving {0} as {1}", source, file);
            if (mode == "append") { JSONUtil.AppendToFile(item, file); }
            else { JSONUtil.WriteFile(item, file); }
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject command = new JObject();
            command.Add("command", "save-json");

            commandEngine.PromptForArgument(command, "file");
            commandEngine.PromptForArgument(command, "source");
            commandEngine.PromptForArgument(command, "item");
            commandEngine.PromptForArgument(command, "mode");

            return command;
        }
    }
}
