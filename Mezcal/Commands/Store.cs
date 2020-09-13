using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public class Store : ICommand
    {
        public void Process(JObject command, Context context)
        {
            var varName = JSONUtil.GetText(command, "#store");
            if (varName == null) { varName = JSONUtil.GetText(command, "name"); }

            var value = JSONUtil.GetToken(command, "value");

            var storeMode = StoreMode.Replace;
            var mode = command["mode"];
            if (mode != null && mode.ToString() == "merge") { storeMode = StoreMode.Merge; }

            if (command["#debug"] != null && command["#debug"].ToString() == "true")
            {
                if (storeMode == StoreMode.Merge)
                {
                    context.Trace($"Merging {JSONUtil.SingleLine(value)} into {varName}");
                }
                else
                {
                    context.Trace($"Setting {varName} to {JSONUtil.SingleLine(value)}");
                }
            }

            context.Store(varName, value, storeMode);
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            throw new NotImplementedException();
        }
    }
}
