using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Mime;
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(text + ": ");
            var value = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;

            var store = command["#store"];
            if (store != null) { context.Store(store.ToString(), value); }

            var responses = command["responses"];
            if (responses != null)
            {
                var jaResponses = (JArray)responses;
                foreach(var jaResponse in jaResponses)
                {
                    var response = jaResponse["response"];
                    if (response == null) { continue; }
                    if (response.ToString() == value)
                    {
                        var commands = jaResponse["commands"];
                        if (commands != null)
                        {
                            var jaCommands = (JArray)commands;
                            foreach(var jaCommand in jaCommands)
                            {
                                context.CommandEngine.RunCommand(jaCommand, context);
                            }
                        }
                    }
                }
            }
        }
    }
}
