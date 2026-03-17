using Motwane.UVSS.Application.Interfaces.HAL;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL.ExternalServices
{
    public class FakeDiagnosticService : IDiagnosticService
    {
        public Task<bool> PingAddressAsync(string ip)
        {
            // Always return success
            return Task.FromResult(true);
        }

        public bool CheckSerialConnection(string portName)
        {
            // Always return success
            return true;
        }
    }
}