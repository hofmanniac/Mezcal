using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    class UIPrompt : ICommand
    {
        public JObject Prompt(CommandEngine commandEngine)
        {
            return null;
        }

        public void Process(JObject command, Context context)
        {
            string text = command["text"].ToString();

            Console.WriteLine(text);
            Console.ReadLine();
        }
    }
}
