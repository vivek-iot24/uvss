using System;
using System.IO.Ports;

namespace Motwane.UVSS.HAL.Sensors
{
    public class SerialSensorService
    {
        private SerialPort _port;
        private string _lastState = "0";

        public event Action VehicleDetected;
        public event Action VehicleLeft;

        public void Start(string port)
        {
            _port = new SerialPort
            {
                PortName = port,
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One
            };

            _port.DataReceived += DataReceived;
            _port.Open();
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string incoming = _port.ReadExisting().Trim();

            if (incoming == "1" && _lastState != "1")
            {
                _lastState = "1";
                VehicleDetected?.Invoke();
            }
            else if (incoming == "0" && _lastState != "0")
            {
                _lastState = "0";
                VehicleLeft?.Invoke();
            }
        }

        public void Stop()
        {
            if (_port?.IsOpen == true)
                _port.Close();
        }
    }
}