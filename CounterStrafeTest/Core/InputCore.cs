using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CounterStrafeTest.Core
{
    public class GameKeyEventArgs : EventArgs
    {
        public Keys LogicKey { get; }
        public Keys PhysicalKey { get; }
        public bool IsDown { get; }
        public GameKeyEventArgs(Keys logic, Keys phys, bool down) { LogicKey = logic; PhysicalKey = phys; IsDown = down; }
    }

    public class InputCore
    {
        private readonly Dictionary<Keys, Keys> _keyMap = new Dictionary<Keys, Keys>();
        private readonly Dictionary<Keys, bool> _keyState = new Dictionary<Keys, bool>();

        // 键盘事件
        public event EventHandler<GameKeyEventArgs> OnGameKeyEvent;
        // 开火事件 (携带高精度时间戳)
        public event Action<long> OnFireEvent;

        public InputCore()
        {
            _keyMap[Keys.W] = Keys.W; _keyMap[Keys.A] = Keys.A;
            _keyMap[Keys.S] = Keys.S; _keyMap[Keys.D] = Keys.D;
        }

        public void Register(IntPtr hwnd)
        {
            var rid = new NativeMethods.RAWINPUTDEVICE[2];
    
            // Keyboard
            rid[0].usUsagePage = 0x01;
            rid[0].usUsage = 0x06;
            rid[0].dwFlags = NativeMethods.RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;

            // Mouse
            rid[1].usUsagePage = 0x01;
            rid[1].usUsage = 0x02;
            rid[1].dwFlags = NativeMethods.RIDEV_INPUTSINK;
            rid[1].hwndTarget = hwnd;

            // 检查注册是否成功
            bool success = NativeMethods.RegisterRawInputDevices(rid, 2, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE)));
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Exception($"RawInput 注册失败，错误代码: {error}");
            }
        }

        public void ProcessMessage(Message m)
        {
            if (m.Msg != NativeMethods.WM_INPUT) return;

            uint dwSize = 0;
            // 获取所需缓冲区大小
            NativeMethods.GetRawInputData(m.LParam, NativeMethods.RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER)));
    
            if (dwSize == 0) return;

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (NativeMethods.GetRawInputData(m.LParam, NativeMethods.RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER))) == dwSize)
                {
                    var raw = Marshal.PtrToStructure<NativeMethods.RAWINPUT>(buffer);
            
                    if (raw.header.dwType == NativeMethods.RIM_TYPEKEYBOARD)
                    {
                        Keys vKey = (Keys)raw.keyboard.VKey;
                        // Flags = 0 或 2 为按下，1 或 3 为松开 (RI_KEY_BREAK 位)
                        bool isDown = (raw.keyboard.Flags & 1) == 0; 
                        HandlePhysicalKey(vKey, isDown);
                    }
                    else if (raw.header.dwType == NativeMethods.RIM_TYPEMOUSE)
                    {
                        if ((raw.mouse.ulButtons & NativeMethods.RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
                        {
                            NativeMethods.QueryPerformanceCounter(out long now);
                            OnFireEvent?.Invoke(now);
                        }
                    }
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        private void HandlePhysicalKey(Keys physKey, bool isDown)
        {
            if (physKey == Keys.F11)
            {
                OnGameKeyEvent?.Invoke(this, new GameKeyEventArgs(Keys.F11, physKey, isDown));
                return;
            }
            if (!_keyMap.ContainsKey(physKey)) return;
            Keys logicKey = _keyMap[physKey];

            if (_keyState.ContainsKey(logicKey) && _keyState[logicKey] == isDown) return;
            _keyState[logicKey] = isDown;

            OnGameKeyEvent?.Invoke(this, new GameKeyEventArgs(logicKey, physKey, isDown));
        }

        public void UpdateMapping(string mappingStr)
        {
            if (mappingStr.Length != 4) return;
            _keyMap.Clear();
            _keyMap[(Keys)Enum.Parse(typeof(Keys), mappingStr[0].ToString())] = Keys.W;
            _keyMap[(Keys)Enum.Parse(typeof(Keys), mappingStr[1].ToString())] = Keys.A;
            _keyMap[(Keys)Enum.Parse(typeof(Keys), mappingStr[2].ToString())] = Keys.S;
            _keyMap[(Keys)Enum.Parse(typeof(Keys), mappingStr[3].ToString())] = Keys.D;
        }
        
        public void ResetMapping()
        {
            _keyMap.Clear();
            _keyMap[Keys.W] = Keys.W; _keyMap[Keys.A] = Keys.A;
            _keyMap[Keys.S] = Keys.S; _keyMap[Keys.D] = Keys.D;
        }
    }
}