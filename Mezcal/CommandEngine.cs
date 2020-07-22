using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mezcal
{
    public class CommandEngine
    {
        private Context _context;
        private List<IModule> Modules { get; set; }

        public CommandEngine()
        {
            this.InitEngine();
        }

        public CommandEngine(string configPath, List<IModule> modules)
        {
            this.InitEngine();
            this.Modules.AddRange(modules);

            var configFile = JSONUtil.ReadFile(configPath);

            string basePath = configFile["$basePath"].ToString();
            this._context.Variables.Add("$basePath", basePath);

            var items = configFile["connections"];

            if (items != null)
            {
                foreach (var item in items)
                {
                    var envConfig = ConnectionConfig.GetConnectionConfig(item);

                    foreach (var module in this.Modules)
                    {
                        var conn = module.ProvideConnection(envConfig);
                        if (conn != null)
                        {
                            this._context.AddConnection(envConfig.Name, conn);

                            if (envConfig.IsDefault == true) { this._context.DefaultConnection = conn; }
                        }
                    }
                }
            }
        }

        private void InitEngine()
        {
            this._context = new Context();
            this._context.CommandEngine = this;
            this.Modules = new List<IModule>();
            this.Modules.Add(new CoreModule());
        }
        public void RunCommandLine()
        {
            bool loop = true;

            while (loop)
            {
                Console.Write(": ");
                string script = Console.ReadLine();

                if (script.Trim().Length == 0) { continue; }
                if (script == "exit") { break; }

                string[] scriptParts = script.Split(' ');
                string commandName = scriptParts[0];

                ICommand comm = ResolveCommand(commandName);
                var jCommand = comm.Prompt(this);

                RunCommand(jCommand, this._context);
            }
        }

        private ICommand ResolveCommand(string commandName)
        {
            ICommand result = null;

            foreach (var module in this.Modules)
            {
                result = module.ResolveCommand(commandName);
                if (result != null) { break; }
            }

            return result;
        }

        public void RunCommand(JObject command, Context context)
        {
            if (command["command"] == null) { return; }
            string commandName = command["command"].ToString();

            ICommand comm = ResolveCommand(commandName);

            try
            {              
                if (comm != null) { comm.Process(command, context); }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CommandEngine: Error running {commandName} - {ex.Message}.");
            }
        }

        public static string GetCommandArgument(JObject command, string argName)
        {
            return JSONUtil.GetText(command, argName);
        }

        public string Prompt(string text)
        {
            Console.Write(text + "\t: ");
            return Console.ReadLine();
        }

        public void PromptForArgument(JObject command, string argName)
        {
            var value = this.Prompt(argName);
            command.Add(argName, value);
        }
    }
}
