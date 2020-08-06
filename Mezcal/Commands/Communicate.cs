using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public class Communicate : ICommand
    {
        public void Process(JObject command, Context context)
        {
            var text = JSONUtil.GetText(command, "communicate");
            if (text == null) { text = JSONUtil.GetText(command, "#communicate"); }

            text = context.ReplaceVariables(text);

            Console.WriteLine(text);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject command = new JObject();
            command.Add("command", "communicate");

            commandEngine.PromptForArgument(command, "text");

            return command;
        }
    }
}
