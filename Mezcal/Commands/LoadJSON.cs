using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Mezcal.Connections;
using System.Net.NetworkInformation;

namespace Mezcal.Commands
{
    public class LoadJSON : ICommand
    {
        public static JToken Create(string file, string set)
        {
            var joCommand = new JObject();
            joCommand.Add("file", file);
            joCommand.Add("set", set);

            return joCommand;
        }

        public void Process(JObject command, Context context)
        {
            // get default property
            var file = JSONUtil.GetText(command, "#load-json");
            if (file == null) { file = JSONUtil.GetText(command, "file"); }

            // get all other properties
            var set = JSONUtil.GetText(command, "set");
            if (set == null) { set = Guid.NewGuid().ToString(); }

            // apply variables to properties (do this when getting property instead?)
            file = context.ReplaceVariables(file);

            // perform the action
            if (command["#debug"] != null && command["#debug"].ToString() == "true")
            {
                context.Trace($"Loading JSON File {file} into {set}");
            }

            
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
