using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TZTMK
{
    public class SerialPortService
    {
        private SerialPort _serialPort;

        public bool IsPortOpen { get; private set; }

        public SerialPortService(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = 8;
        }

        public bool OpenPort()
        {
            try
            {
                _serialPort.Open();
                IsPortOpen = true;
                return true;
            }
            catch (Exception)
            {
                return false;
                
            }
        }

        public void ClosePort()
        {
            _serialPort.Close();
            IsPortOpen = false;
        }

        public void SendString(string dataString)
        {
            _serialPort.WriteLine(dataString);
        }

        public void SendBytes(byte[] byteArray,int offset,int count)
        {
            _serialPort.Write(byteArray, offset, count);
        }
    }
}
