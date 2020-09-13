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
            // get default property
            var item = JSONUtil.GetToken(command, "communicate");
            if (item == null) { item = JSONUtil.GetToken(command, "#communicate"); }
            if (item == null) { item = JSONUtil.GetToken(command, "#say"); }

            // get all other properties
            var select = JSONUtil.GetText(command, "select");
            var rate = JSONUtil.GetInt32(command, "rate");

            item = context.ReplaceVariables(item);

            if (select != null)
            {
                item = item.SelectToken(select);
            }

            if (rate != null)
            {
                var iRate = (int)rate; // get it? :)
                foreach(char c in item.ToString())
                {
                    Console.Write(c);
                    System.Threading.Thread.Sleep(iRate);
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(item);
            }
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
