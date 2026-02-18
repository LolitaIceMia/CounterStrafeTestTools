using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CounterStrafeTest.Core
{
    // 定义按键事件参数
    public class GameKeyEventArgs : EventArgs
    {
        public Keys LogicKey { get; } // 映射后的逻辑键 (WASD)
        public Keys PhysicalKey { get; } // 物理键
        public bool IsDown { get; }
        public GameKeyEventArgs(Keys logic, Keys phys, bool down) { LogicKey = logic; PhysicalKey = phys; IsDown = down; }
    }

    public class InputCore
    {
        private readonly Dictionary<Keys, Keys> _keyMap = new Dictionary<Keys, Keys>();
        private readonly Dictionary<Keys, bool> _keyState = new Dictionary<Keys, bool>();

        public event EventHandler<GameKeyEventArgs> OnGameKeyEvent;

        public InputCore()
        {
            // 默认映射
            _keyMap[Keys.W] = Keys.W; _keyMap[Keys.A] = Keys.A;
            _keyMap[Keys.S] = Keys.S; _keyMap[Keys.D] = Keys.D;
        }

        public void Register(IntPtr hwnd)
        {
            var rid = new NativeMethods.RAWINPUTDEVICE[1];
            rid[0].usUsagePage = 0x01;
            rid[0].usUsage = 0x06;
            rid[0].dwFlags = NativeMethods.RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;
            NativeMethods.RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE)));
        }

        public void ProcessMessage(Message m)
        {
            if (m.Msg != NativeMethods.WM_INPUT) return;

            uint dwSize = 0;
            NativeMethods.GetRawInputData(m.LParam, NativeMethods.RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER)));
            
            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (NativeMethods.GetRawInputData(m.LParam, NativeMethods.RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER))) == dwSize)
                {
                    var raw = Marshal.PtrToStructure<NativeMethods.RAWINPUT>(buffer);
                    if (raw.header.dwType == NativeMethods.RIM_TYPEKEYBOARD)
                    {
                        Keys vKey = (Keys)raw.keyboard.VKey;
                        bool isDown = (raw.keyboard.Flags & 1) == 0;
                        HandlePhysicalKey(vKey, isDown);
                    }
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        private void HandlePhysicalKey(Keys physKey, bool isDown)
        {
            if (!_keyMap.ContainsKey(physKey)) return;
            Keys logicKey = _keyMap[physKey];

            // 状态去重
            if (_keyState.ContainsKey(logicKey) && _keyState[logicKey] == isDown) return;
            _keyState[logicKey] = isDown;

            // 触发事件
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
    }
}