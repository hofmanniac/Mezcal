using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public class RunScript : ICommand
    {
        public void Process(JObject command, Context context)
        {
            string file = command["script"].ToString();

            file = context.ReplaceVariables(file);

            JArray items = (JArray)Connections.JSONUtil.ReadFile(file);

            if (items == null)
            {
                Console.WriteLine($"RunScript: Unable to read file {file}.");
            }
            else
            {
                foreach (var item in items)
                {
                    if (item["enabled"] != null)
                    {
                        var enabled = item["enabled"].ToString();
                        if (enabled == "false") { continue; }
                    }

                    context.CommandEngine.RunCommand((JObject)item, context);
                }
            }
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject command = new JObject();
            command.Add("command", "run-script");

            commandEngine.PromptForArgument(command, "script");

            return command;
        }
    }
}
