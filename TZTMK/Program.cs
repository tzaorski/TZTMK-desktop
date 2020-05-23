using KeyMouseAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TZTMK
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string portName = ConfigurationManager.AppSettings.Get("PortName");
            string baudRate = ConfigurationManager.AppSettings.Get("BaudRate");
            AppObjects.PortName = portName;
            AppObjects.BaudRate = baudRate;
            int.TryParse(baudRate, out int baudRateAsInt);

            // do obsługi transmisji 
            AppObjects.KeyMouseToolkit = new KeyMouseToolkit();
            AppObjects.KeyMouseToolkit.CreateKeyboardHook();
            AppObjects.PortService = new SerialPortService(AppObjects.PortName, baudRateAsInt);
            AppObjects.ArduService = new ArduCommService();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
