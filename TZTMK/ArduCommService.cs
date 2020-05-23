using KeyMouseAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TZTMK
{
    public class ArduCommService
    {

        /// <summary>
        /// Transmisja naciśnięcia klawisza myszy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void MouseButtonTransmission(object sender, MouseButtonActivityEventArgs ea)
        {
            string singleStringToSend;
            switch (ea.KeyCode)
            {
                case KeyCode.LButton:
                    singleStringToSend = "L";
                    break;
                case KeyCode.RButton:
                    singleStringToSend = "R";
                    break;
                case KeyCode.MButton:
                    singleStringToSend = "M";
                    break;
                default:
                    return; // wyjście bez dalszych akcji
            }
            if (!ea.IsPressed)
            {
                singleStringToSend = singleStringToSend.ToLower();
            }
            AppObjects.PortService.SendString(singleStringToSend);
        }

        /// <summary>
        /// Transmisja ruchu myszki
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void MouseMoveTransmission(object sender, MouseMoveActivityEventArgs ea)
        {
            var diffXasByte = ConvertIntToOffsetedByte(ea.DiffX);
            var diffYasByte = ConvertIntToOffsetedByte(ea.DiffY);
            byte[] byteArray = new byte[] { 80, diffXasByte, diffYasByte };         // 80 to kod znaku P
            AppObjects.PortService.SendBytes(byteArray, 0, 3);
        }

        /// <summary>
        /// Transmisja ruchu kółkiem myszki
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void MouseWheelTransmission(object sender, MouseWheelActivityEventArgs ea)
        {

            byte actionByte;
            if (ea.RotationDown)
            {
                actionByte = (byte)'S';
            }
            else
            {
                actionByte = (byte)'s';
            }

            byte[] byteArray = new byte[] { actionByte };
            AppObjects.PortService.SendBytes(byteArray, 0, 1);
        }

        /// <summary>
        /// Transmisja klawisza klawiatury
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void KeyTransmission(object sender, KeyActivityEventArgs ea)
        {
            byte actionByte;

            if (ea.IsPressed)
            {
                actionByte = (byte)'K';
            }
            else
            {
                actionByte = (byte)'k';
            }
            byte keyByte = MapKeyToArduinoKeycodeByte(ea.KeyCode);
            byte[] byteArray = new byte[] { actionByte, keyByte };
            AppObjects.PortService.SendBytes(byteArray, 0, 2);
        }

        /// <summary>
        /// Mapuje kody klawaitury systemu Windows na kody klawiszy Arduino/HID
        /// </summary>
        /// <param name="inputKeyCode"></param>
        /// <returns></returns>
        private byte MapKeyToArduinoKeycodeByte(KeyCode inputKeyCode)
        {
            byte inputKeyCodeByte = (byte)inputKeyCode;
            // jak nic ciekawego nie przyszło to wyjście
            if (inputKeyCodeByte == 0)
            {
                return 0;
            }
            byte result;
            // to można przemapować automatycznie, bo to tylko przesunięcie
            // znaki A-Z
            if (inputKeyCodeByte >= 65 && inputKeyCodeByte <= 90)
            {
                result = (byte)(inputKeyCodeByte + 32);
                return result;
            }
            // to także automatycznie, bo też tylko przesunięcie
            // cyfry
            if (inputKeyCodeByte >= 48 && inputKeyCodeByte <= 57)
            {
                result = inputKeyCodeByte;
                return result;
            }
            // kolejne znaki juz trzeba ręcznie mapować
            // najpierw robimy mapę do mapowania
            var map = new Dictionary<byte, byte>();
            map.Add(162, 128);        // KEY_LEFT_CTRL
            map.Add(160, 129);  //KEY_LEFT_SHIFT
            map.Add(164, 130);  //KEY_LEFT_ALT
            map.Add(91, 131);  //KEY_LEFT_GUI
            map.Add(163, 132);  //KEY_RIGHT_CTRL
            map.Add(161, 133);  //KEY_RIGHT_SHIFT
            map.Add(165, 134);  //KEY_RIGHT_ALT
            map.Add(92, 135);  //KEY_RIGHT_GUI
            map.Add(13, 176);  //KEY_RETURN
            map.Add(27, 177);  //KEY_ESC
            map.Add(8, 178);  //KEY_BACKSPACE
            map.Add(9, 179);        // TAB
            map.Add(189, 181);  // zwykly -_
            map.Add(187, 182);  // zwykly +=
            map.Add(219, 183);  // {[
            map.Add(221, 184);  // }]
            map.Add(220, 185);  // pipe
            map.Add(186, 187);  // semicolon
            map.Add(222, 188);  // "
            map.Add(192, 189);  // tylda
            map.Add(188, 190);  // <
            map.Add(190, 191);  // >
            map.Add(191, 192);  // question mark
            map.Add(20, 193);  //KEY_CAPS_LOCK
            map.Add(112, 194);  //KEY_F1
            map.Add(113, 195);  //KEY_F2
            map.Add(114, 196);  //KEY_F3
            map.Add(115, 197);  //KEY_F4
            map.Add(116, 198);  //KEY_F5
            map.Add(117, 199);  //KEY_F6
            map.Add(118, 200);  //KEY_F7
            map.Add(119, 201);  //KEY_F8
            map.Add(120, 202);  //KEY_F9
            map.Add(121, 203);  //KEY_F10
            map.Add(122, 204);  //KEY_F11
            map.Add(123, 205);  //KEY_F12
            map.Add(44, 206);  //print screen
            map.Add(19, 208); // pause
            map.Add(45, 209);  //KEY_INSERT
            map.Add(36, 210);  //KEY_HOME
            map.Add(33, 211);  //KEY_PAGE_UP
            map.Add(46, 212);  //KEY_DELETE
            map.Add(35, 213);  //KEY_END
            map.Add(34, 214);  //KEY_PAGE_DOWN
            map.Add(39, 215);  //KEY_RIGHT_ARROW
            map.Add(37, 216);  //KEY_LEFT_ARROW
            map.Add(40, 217);  //KEY_DOWN_ARROW
            map.Add(38, 218);  //KEY_UP_ARROW
            map.Add(144, 219);  // num lock
            map.Add(111, 220);  // / numeryczna
            map.Add(106, 221);  // * numeryczna
            map.Add(109, 222);  // - numeryczna
            map.Add(107, 223);  // + numeryczna
            map.Add(97, 225);  // 1 numeryczna
            map.Add(98, 226);  // 2 numeryczna
            map.Add(99, 227);  // 3 numeryczna
            map.Add(100, 228);  // 4 numeryczna
            map.Add(101, 229);  // 5 numeryczna
            map.Add(102, 230);  // 6 numeryczna
            map.Add(103, 231);  // 7 numeryczna
            map.Add(104, 232);  // 8 numeryczna
            map.Add(105, 233);  // 9 numeryczna
            map.Add(96, 234);  // 0 numeryczna
            map.Add(110, 235); // . Del numeryczna
            map.Add(93, 237); // klawisz konktekstowego menu
            map.Add(32, 32); // spacja

            // teraz mapujemy
            map.TryGetValue(inputKeyCodeByte, out byte mappingResult);
            result = mappingResult;
            return result;
        }

        private byte ConvertIntToOffsetedByte(int number)
        {
            if (number < -128)
            {
                number = 128;
            }
            if (number > 127)
            {
                number = 127;
            }
            number += 128;  // offset dla transmisji
            var result = (byte)number;
            return result;
        }
    }
}
