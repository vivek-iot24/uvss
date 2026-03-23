using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace Motwane.UVSS.HAL
{
    public class SerialSensorService : ISensorService
    {
        private SerialPort _serialPort;

        public event Action<string> OnSignalReceived;

        public void Start()
        {
            _serialPort = new SerialPort
            {
                PortName = "COM3",
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
        }

        public void Stop()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var data = _serialPort.ReadExisting().Trim();

                if (!string.IsNullOrEmpty(data))
                {
                    OnSignalReceived?.Invoke(data);
                }
            }
            catch
            {
                // swallow or log later
            }
        }
    }
}