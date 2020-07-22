using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Commands
{
    public interface ICommand
    {
        void Process(JObject command, Context context);

        JObject Prompt(CommandEngine commandEngine);
    }
}
