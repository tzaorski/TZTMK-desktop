using System;
using System.Collections.Generic;
using System.Text;

namespace KeyMouseAPI.Model
{
    public class MouseWheelActivityEventArgs : EventArgs
    {
        public bool RotationDown { get; set; }
    }
}
