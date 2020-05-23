using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyMouseAPI.Model
{
    public class MouseMoveActivityEventArgs : EventArgs
    {
        public int DiffX { get; set; }
        public int DiffY { get; set; }
    }
}
