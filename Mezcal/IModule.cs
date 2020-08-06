using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal
{
    public interface IModule
    {
        IConnection ProvideConnection(ConnectionConfig envConfig);

        ICommand ResolveCommand(JObject joCommand);
    }
}
