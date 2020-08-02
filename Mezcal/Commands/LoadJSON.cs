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
            string file = command["file"].ToString();
            string set = command["set"].ToString();

            file = context.ReplaceVariables(file);

            //Console.WriteLine("Loading JSON File {0} into {1}", file, set);
            var sub = JSONUtil.ReadFile(file);
            context.Store(set, sub);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            return null;
        }
    }
}
