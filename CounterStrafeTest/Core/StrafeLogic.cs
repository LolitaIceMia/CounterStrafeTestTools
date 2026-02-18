using System;
using System.Windows.Forms;

namespace CounterStrafeTest.Core
{
    public class StrafeResult
    {
        public string Axis { get; set; } // "AD" or "WS"
        public double Latency { get; set; }
        public Keys ReleaseKey { get; set; }
        public Keys PressKey { get; set; }
        public bool IsValid => Math.Abs(Latency) < 1000; // 扩大有效范围防止漏测
    }

    public class StrafeLogic
    {
        private long _freq;
        
        // 记录按键最后一次操作的时间戳
        private long _lastPressA = 0, _lastReleaseA = 0;
        private long _lastPressD = 0, _lastReleaseD = 0;
        private long _lastPressW = 0, _lastReleaseW = 0;
        private long _lastPressS = 0, _lastReleaseS = 0;

        // 记录按键当前物理状态
        private bool _isDownA = false;
        private bool _isDownD = false;
        private bool _isDownW = false;
        private bool _isDownS = false;

        public StrafeLogic()
        {
            NativeMethods.QueryPerformanceFrequency(out _freq);
        }

        public void Reset()
        {
            _lastPressA = _lastReleaseA = 0;
            _lastPressD = _lastReleaseD = 0;
            _lastPressW = _lastReleaseW = 0;
            _lastPressS = _lastReleaseS = 0;
            _isDownA = _isDownD = _isDownW = _isDownS = false;
        }

        public StrafeResult ProcessLogicKey(Keys key, bool isDown)
        {
            NativeMethods.QueryPerformanceCounter(out long now);
            
            // 1. 更新状态与时间戳
            UpdateState(key, isDown, now);

            // 2. 检测急停 (核心逻辑)
            // 只有当发生 "按下" 或 "松开" 事件时，才可能触发一次急停判定
            // 我们检查 AD 轴和 WS 轴
            if (key == Keys.A || key == Keys.D) return CheckAxis(now, Keys.A, Keys.D, _lastReleaseA, _lastPressD, _lastReleaseD, _lastPressA, _isDownA, _isDownD, "AD");
            if (key == Keys.W || key == Keys.S) return CheckAxis(now, Keys.W, Keys.S, _lastReleaseW, _lastPressS, _lastReleaseS, _lastPressW, _isDownW, _isDownS, "WS");

            return null;
        }

        private void UpdateState(Keys key, bool isDown, long now)
        {
            if (key == Keys.A) { _isDownA = isDown; if (isDown) _lastPressA = now; else _lastReleaseA = now; }
            else if (key == Keys.D) { _isDownD = isDown; if (isDown) _lastPressD = now; else _lastReleaseD = now; }
            else if (key == Keys.W) { _isDownW = isDown; if (isDown) _lastPressW = now; else _lastReleaseW = now; }
            else if (key == Keys.S) { _isDownS = isDown; if (isDown) _lastPressS = now; else _lastReleaseS = now; }
        }

        private StrafeResult CheckAxis(long now, Keys k1, Keys k2, long rel1, long press2, long rel2, long press1, bool down1, bool down2, string axis)
        {
            double ms = 0;
            Keys releaseKey = Keys.None;
            Keys pressKey = Keys.None;
            bool detected = false;

            // 情况 A: 完美/慢速急停 (Gap)
            // 逻辑: 刚刚按下了 k2, 且 k1 已经在不久前松开了
            // 时间差 = Press(k2) - Release(k1)  (正数)
            if (down2 && !down1 && press2 == now) 
            {
                // 检查 k1 是否刚松开不久 (例如 500ms 内)
                if (now - rel1 < _freq / 2) 
                {
                    ms = (double)(press2 - rel1) * 1000.0 / _freq;
                    releaseKey = k1; pressKey = k2;
                    detected = true;
                }
            }
            // 对称情况
            else if (down1 && !down2 && press1 == now)
            {
                if (now - rel2 < _freq / 2)
                {
                    ms = (double)(press1 - rel2) * 1000.0 / _freq;
                    releaseKey = k2; pressKey = k1;
                    detected = true;
                }
            }

            // 情况 B: 重叠急停 (Overlap)
            // 逻辑: 刚刚松开了 k1, 但 k2 早就按下了 (且 k2 还在按着)
            // 时间差 = Press(k2) - Release(k1) (负数)
            // 注意: 这里触发点是 "松开 k1 的瞬间"
            else if (!down1 && down2 && rel1 == now)
            {
                // 检查 k2 是否在按着
                if (now - press2 < _freq / 2)
                {
                    ms = (double)(press2 - rel1) * 1000.0 / _freq;
                    releaseKey = k1; pressKey = k2;
                    detected = true;
                }
            }
            // 对称情况
            else if (!down2 && down1 && rel2 == now)
            {
                if (now - press1 < _freq / 2)
                {
                    ms = (double)(press1 - rel2) * 1000.0 / _freq;
                    releaseKey = k2; pressKey = k1;
                    detected = true;
                }
            }

            if (detected)
            {
                return new StrafeResult { Axis = axis, Latency = ms, ReleaseKey = releaseKey, PressKey = pressKey };
            }
            return null;
        }
    }
}