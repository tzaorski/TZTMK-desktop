using KeyMouseAPI.Model;
using KeyMouseAPI.System;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyMouseAPI
{
    public class KeyMouseToolkit : IDisposable
    {
        private IntPtr globalKeyboardHookId = IntPtr.Zero;
        private IntPtr globalMouseHookId = IntPtr.Zero;
        private readonly IntPtr currentModuleId;
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private User32.LowLevelHook hookKeyboardDelegate;
        private User32.LowLevelHook hookMouseDelegate;
        private int baseMouseX;
        private int baseMouseY;
        private bool systemKeyActionActive = false;     // do przesyłania polskich znaków które są przesyłane jako kombinacja klawiszy

        public bool RemoteControlActive { get; private set; }
        public bool CanActivateRemoteControl { get; set; }

        public event EventHandler<KeyActivityEventArgs> KeyActivity;
        public event EventHandler<MouseButtonActivityEventArgs> MouseButtonActivity;
        public event EventHandler<MouseMoveActivityEventArgs> MouseMoveActivity;
        public event EventHandler<MouseWheelActivityEventArgs> MouseWheelActivity;

        public KeyMouseToolkit()
        {
            // sprawdź status scroll locka
            var scrollKeyState = User32.GetKeyState(KeyCode.Scroll);
            // jeśli scroll lock jest włączony to wyłącz
            if (scrollKeyState == 1)
            {
                const int KEYEVENTF_EXTENDEDKEY = 0x1;
                const int KEYEVENTF_KEYUP = 0x2;
                User32.keybd_event(0x91, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                User32.keybd_event(0x91, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                (UIntPtr)0);
            }
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModudle = currentProcess.MainModule;
            this.currentModuleId = User32.GetModuleHandle(currentModudle.ModuleName);
        }

        public void CreateKeyboardHook()
        {
            if (this.globalKeyboardHookId == IntPtr.Zero)
            {
                // klawiatura
                this.hookKeyboardDelegate = HookKeyboardCallbackImplementation;
                this.globalKeyboardHookId = User32.SetWindowsHookEx(WH_KEYBOARD_LL, this.hookKeyboardDelegate, this.currentModuleId, 0);
                // mysz
                this.hookMouseDelegate = HookMouseCallbackImplementation;
                this.globalMouseHookId = User32.SetWindowsHookEx(WH_MOUSE_LL, this.hookMouseDelegate, this.currentModuleId, 0);
            }
        }

        protected void OnKeyActivity(KeyActivityEventArgs e)
        {
            EventHandler<KeyActivityEventArgs> handler = KeyActivity;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMouseButtonActivity(MouseButtonActivityEventArgs e)
        {
            EventHandler<MouseButtonActivityEventArgs> handler = MouseButtonActivity;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMouseMoveActivity(MouseMoveActivityEventArgs e)
        {
            EventHandler<MouseMoveActivityEventArgs> handler = MouseMoveActivity;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMouseWheelActivity(MouseWheelActivityEventArgs e)
        {
            EventHandler<MouseWheelActivityEventArgs> handler = MouseWheelActivity;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private IntPtr HookMouseCallbackImplementation(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (RemoteControlActive)
            {
                var wmMouse = (User32.MouseMessage)wParam;

                if (wmMouse == User32.MouseMessage.WM_MOUSEMOVE)
                {
                    User32.POINT lpPoint = (User32.POINT)Marshal.PtrToStructure(lParam, typeof(User32.POINT));
                    MouseMoveActivityEventArgs args = new MouseMoveActivityEventArgs();
                    args.DiffX = lpPoint.x - baseMouseX;
                    args.DiffY = lpPoint.y - baseMouseY;
                    OnMouseMoveActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_MOUSEWHEEL)
                {
                    User32.MSLLHOOKSTRUCT mSLLHOOKSTRUCT = (User32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
                    var wheelData = ((short)(mSLLHOOKSTRUCT.mouseData >> 16));
                    MouseWheelActivityEventArgs args = new MouseWheelActivityEventArgs();
                    args.RotationDown = (wheelData > 0) ? false : true;
                    OnMouseWheelActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_LBUTTONDOWN)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = true;
                    args.KeyCode = KeyCode.LButton;
                    OnMouseButtonActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_LBUTTONUP)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = false;
                    args.KeyCode = KeyCode.LButton;
                    OnMouseButtonActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_RBUTTONDOWN)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = true;
                    args.KeyCode = KeyCode.RButton;
                    OnMouseButtonActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_RBUTTONUP)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = false;
                    args.KeyCode = KeyCode.RButton;
                    OnMouseButtonActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_MBUTTONDOWN)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = true;
                    args.KeyCode = KeyCode.MButton;
                    OnMouseButtonActivity(args);
                }

                if (wmMouse == User32.MouseMessage.WM_MBUTTONUP)
                {
                    MouseButtonActivityEventArgs args = new MouseButtonActivityEventArgs();
                    args.IsPressed = false;
                    args.KeyCode = KeyCode.MButton;
                    OnMouseButtonActivity(args);
                }
                // bo nie puszczamy dalej, tylko wysłane zostało na port szeregowy
                return (IntPtr)1;

            }
            // puszczamy dalej (nie łapiemy) do obsługi
            return User32.CallNextHookEx(globalMouseHookId, nCode, wParam, lParam);
        }

        private IntPtr HookKeyboardCallbackImplementation(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int wParamAsInt = wParam.ToInt32();
            bool keyDown = (wParamAsInt == WM_KEYDOWN);
            bool sysKeyDown = (wParamAsInt == WM_SYSKEYDOWN);
            var keyboardKey = (KeyCode)Marshal.ReadInt32(lParam);

            if (keyboardKey == KeyCode.Scroll)
            {
                // w przypadku klawisza scroll lock którego i tak nie wysyłamy, interesuje nas tylko przyciśnięcie
                if (keyDown)
                {
                    // toggle zdalne sterowanie
                    if (RemoteControlActive)
                    {
                        RemoteControlActive = false;
                    }
                    else
                    {
                        // jeśli jest zezwolenie na aktywację zdalnego sterowania
                        if (CanActivateRemoteControl)
                        {
                            RemoteControlActive = true;
                            // pobierz bazową pozycję dla myszy
                            User32.GetCursorPos(out User32.POINT lpPoint);
                            baseMouseX = lpPoint.x;
                            baseMouseY = lpPoint.y;
                        }
                    }
                }
                // puszczamy dalej (nie łapiemy) scroll locka aby mieć zapaloną lampkę na klawiaturze
                return User32.CallNextHookEx(globalKeyboardHookId, nCode, wParam, lParam);
            }
            else
            {
                if (RemoteControlActive)
                {
                    // wywołaj event i pozniej (jak jest podbindowana) wysyłka klawisza przez port
                    KeyActivityEventArgs args = new KeyActivityEventArgs();
                    args.IsPressed = keyDown | sysKeyDown;
                    args.KeyCode = keyboardKey;

                    // prawy ALT jest przesyłany jako kombinacja z Ctrl lewym i tutaj trzeba pokombinować, stąd poniższe 4 kroki

                    // 1 - poczatek transmisji prawego ALTa - najpierw leci Ctrl lewy
                    if (systemKeyActionActive == false && !keyDown && sysKeyDown && keyboardKey == KeyCode.LControlKey)
                    {
                        systemKeyActionActive = true;
                        // ignorujemy i nie przesylamy dalej
                        return (IntPtr)1;
                    }

                    // 2 - kolejny etap transmisji prawego ALTa - będzie obsłużony normalnie jak zwykły klawisz, więc nie obkodowane

                    // 3
                    if (systemKeyActionActive == true && !keyDown && !sysKeyDown && keyboardKey == KeyCode.LControlKey)
                    {
                        systemKeyActionActive = false;
                        // ignorujemy i nie przesylamy dalej
                        return (IntPtr)1;
                    }

                    // 4 etapu tez nie trzeba obkodowywać bo też będzie obsłużony normalnie

                    // przesylamy dalej
                    OnKeyActivity(args);

                    //Return a dummy value to trap the keystroke - łapiemy i nie puszczamy dalej do obsługi
                    return (IntPtr)1;
                }
                else
                {
                    // puszczamy dalej (nie łapiemy) do obsługi
                    return User32.CallNextHookEx(globalKeyboardHookId, nCode, wParam, lParam);
                }
            }

        }

        public void Dispose()
        {
            RemoteControlActive = false;
            if (globalKeyboardHookId != IntPtr.Zero)
                User32.UnhookWindowsHookEx(globalKeyboardHookId);
            if (globalMouseHookId != IntPtr.Zero)
                User32.UnhookWindowsHookEx(globalMouseHookId);
        }
    }
}
