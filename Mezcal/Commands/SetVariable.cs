using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public class SetVariable : ICommand
    {
        public void Process(JObject command, Context context)
        {
            var varName = JSONUtil.GetText(command, "#set-variable");
            if (varName == null) { varName = JSONUtil.GetText(command, "name"); }

            var value = JSONUtil.GetToken(command, "value");

            context.Trace($"Setting {varName} to {value}");
            context.Store(varName, value);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            throw new NotImplementedException();
        }
    }
}
