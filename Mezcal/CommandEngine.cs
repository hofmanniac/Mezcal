using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mezcal
{
    public class CommandEngine
    {
        private Context _context;
        public List<IModule> Modules { get; set; }

        public Context Context {get {return this._context;} }

        public CommandEngine()
        {
            this.InitEngine();

            // look for init.json in .exe path
            var exePath = AppContext.BaseDirectory;
            var initFile = JSONUtil.ReadFile(exePath + "\\init.json");
            if (initFile != null) // if it's there
            {
                this._context.Trace("ini.json found - loaded");

                this._context.Store("#init", initFile);

                var joCommand = new JObject();
                joCommand.Add("command", "run-rules");
                var joQuery = new JObject();
                joQuery.Add("#event", "engine-init");
                joCommand.Add("query", joQuery);

                this.RunCommand(joCommand, this._context);
            }
        }

        public CommandEngine(string configPath)
        {
            this.InitEngine();

            var configFile = JSONUtil.ReadFile(configPath);

            var variables = configFile["variables"];

            if (variables != null)
            {
                foreach(JObject variable in variables)
                {
                    var varProp = variable.Properties().FirstOrDefault();
                    var varKey = varProp.Name;
                    var varValue = varProp.Value;
                    this._context.Variables.Add(varKey, varValue);
                }
            }
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

            string appPath = AppContext.BaseDirectory + @"..\..";
            this._context.Variables.Add("$appPath", appPath);
        }

        public void RunCommandLine()
        {
            bool loop = true;

            while (loop)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(": ");
                string script = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;

                if (script.Trim().Length == 0) { continue; }
                if (script == "exit") { break; }

                var jCommand = new JObject();
                jCommand.Add("command", "run-rules");
                var joInput = new JObject();
                joInput.Add("input", script);
                jCommand.Add("query", joInput);

                //string[] scriptParts = script.Split(' ');
                //string commandName = scriptParts[0];

                //ICommand comm = ResolveCommand(commandName);
                //if (comm == null) { Console.WriteLine($"Command '{commandName}' does not resolve."); continue; }
                //var jCommand = comm.Prompt(this);

                this.RunCommand(jCommand, this._context);

                Console.WriteLine();
            }
        }

        public void RunCommand(JToken jtCommand, Context context)
        {
            if (jtCommand.Type == JTokenType.Object)
            {
                jtCommand = context.Resolve(jtCommand);
                var joCommand = (JObject)jtCommand;

                //if (command["command"] == null) { return; }
                //string commandName = command["command"].ToString();

                if (joCommand["enabled"] != null)
                {
                    var enabled = joCommand["enabled"].ToString();
                    if (enabled == "false") { return; }
                }

                ICommand comm = ResolveCommand(joCommand);
                if (comm == null) { Console.WriteLine($"Warning - unable to resolve command {joCommand}"); return; }

                try
                {
                    comm.Process(joCommand, context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CommandEngine: Error running {joCommand} - {ex.Message}.");
                }
            }
        }

        //public void RunCommand(JObject command, Context context)
        //{
        //    //if (command["command"] == null) { return; }
        //    //string commandName = command["command"].ToString();

        //    if (command["enabled"] != null)
        //    {
        //        var enabled = command["enabled"].ToString();
        //        if (enabled == "false") { return; }
        //    }

        //    ICommand comm = ResolveCommand(command);
        //    if (comm == null) { Console.WriteLine($"Warning - unable to resolve command {command}");  return; }

        //    try
        //    {
        //        command = context.Resolve(command);
        //        comm.Process(command, context);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"CommandEngine: Error running {command} - {ex.Message}.");
        //    }
        //}

        private ICommand ResolveCommand(JObject joCommand)
        {
            ICommand result = null;

            foreach (var module in this.Modules)
            {
                result = module.ResolveCommand(joCommand);
                if (result != null) { break; }
            }

            return result;
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
