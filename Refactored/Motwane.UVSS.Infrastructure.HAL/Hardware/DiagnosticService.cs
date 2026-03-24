using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motwane.UVSS.Application.Interfaces.HAL;
using System.IO.Ports;
using System.Net.NetworkInformation;


namespace Motwane.UVSS.Infrastructure.HAL.Hardware
{
    public class DiagnosticService : IDiagnosticService
    {
        public async Task<bool> PingAddressAsync(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool CheckSerialConnection(string portName)
        {
            try
            {
                using (SerialPort serialPort = new SerialPort(portName))
                {
                    serialPort.Open();
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}