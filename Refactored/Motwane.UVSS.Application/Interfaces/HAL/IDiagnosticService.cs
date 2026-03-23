using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Application.Interfaces.HAL
{
    public interface IDiagnosticService
    {
        Task<bool> PingAddressAsync(string ip);

        bool CheckSerialConnection(string portName);
    }
}