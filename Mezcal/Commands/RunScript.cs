using Mezcal.Connections;
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
            string file = JSONUtil.GetText(command, "#run-script");
            if (file == null) { file = command["script"].ToString(); }
            file = context.ReplaceVariables(file);

            System.IO.FileInfo fi = new System.IO.FileInfo(file);
            var scriptDir = fi.Directory.FullName;

            JArray items = (JArray)JSONUtil.ReadFile(file);

            if (items == null)
            {
                Console.WriteLine($"RunScript: Unable to read file {file}.");
                return;
            }

            foreach (var item in items)
            {
                var commandItem = Unification.Replace(item, "#scriptDir", scriptDir);
                context.CommandEngine.RunCommand(commandItem, context);
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
