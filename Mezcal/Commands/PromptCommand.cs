using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    class PromptCommand : ICommand
    {
        public JObject Prompt(CommandEngine commandEngine)
        {
            return null;
        }

        public void Process(JObject command, Context context)
        {
            var text = command["#prompt"];
            if (text == null) { text = command["text"]; }
            if (text == null) { return; }

            Console.Write(text + ": ");
            var value = Console.ReadLine();

            var store = command["store"];
            if (store != null) { context.Store(store.ToString(), value); }
        }
    }
}
