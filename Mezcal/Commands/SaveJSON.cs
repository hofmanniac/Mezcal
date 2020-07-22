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
            string file = CommandEngine.GetCommandArgument(command, "file");
            string source = CommandEngine.GetCommandArgument(command, "source");

            file = context.ReplaceVariables(file);

            JToken sourceToken = (JToken)context.Fetch(source);

            Console.WriteLine("Saving {0} as {1}", source, file);
            Connections.JSONUtil.WriteFile(sourceToken, file);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject command = new JObject();
            command.Add("command", "save-json");

            commandEngine.PromptForArgument(command, "file");
            commandEngine.PromptForArgument(command, "source");

            return command;
        }
    }
}
