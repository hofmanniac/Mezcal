using Mezcal.Commands;
using Mezcal.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal
{
    public interface IModule
    {
        IConnection ProvideConnection(ConnectionConfig envConfig);

        ICommand ResolveCommand(string commandName);
    }
}
