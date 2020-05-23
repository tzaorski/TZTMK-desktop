using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyMouseAPI.Model
{
    public class MouseButtonActivityEventArgs : EventArgs
    {
        public bool IsPressed { get; set; }     // kiedy true przyciśnięty, a jak false to zwolniony
        public KeyCode KeyCode { get; set; }    // ponieważ klawisze myszki także mają swoje kody klawiszy
    }
}
