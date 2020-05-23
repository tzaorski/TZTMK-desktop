using KeyMouseAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TZTMK
{
    internal static class AppObjects
    {
        public static KeyMouseToolkit KeyMouseToolkit { get; set; }
        public static SerialPortService PortService { get; set; }
        public static ArduCommService ArduService { get; set; }
        public static string PortName { get; set; }
        public static string BaudRate { get; set; }
    }
}
