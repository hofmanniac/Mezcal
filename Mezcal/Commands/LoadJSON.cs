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
            string into = command["into"].ToString();

            file = context.ReplaceVariables(file);

            Console.WriteLine("Loading JSON File {0} into {1}", file, into);
            var sub = JSONUtil.ReadFile(file);
            context.Store(into, sub);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            return null;
        }
    }
}
