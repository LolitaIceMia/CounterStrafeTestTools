using System;
using System.Windows.Forms;

namespace CounterStrafeTest.Core
{
    public class StrafeResult
    {
        public string Axis { get; set; }
        public double Latency { get; set; }
        public Keys ReleaseKey { get; set; }
        public Keys PressKey { get; set; }
        // 新增：急停完成时的精确时刻 (QPC Ticks)
        public long StopTick { get; set; } 
        public bool IsValid => Math.Abs(Latency) < 1000;
    }

    public class StrafeLogic
    {
        private long _freq;
        public long Frequency => _freq; // 公开频率供外部计算时间

        private long _lastPressA = 0, _lastReleaseA = 0;
        private long _lastPressD = 0, _lastReleaseD = 0;
        private long _lastPressW = 0, _lastReleaseW = 0;
        private long _lastPressS = 0, _lastReleaseS = 0;

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
            UpdateState(key, isDown, now);

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
            long stopTick = now;

            // Gap Mode: down2 && !down1 && press2 == now
            if (down2 && !down1 && press2 == now) 
            {
                if (now - rel1 < _freq / 2) 
                {
                    ms = (double)(press2 - rel1) * 1000.0 / _freq;
                    releaseKey = k1; pressKey = k2;
                    detected = true;
                }
            }
            else if (down1 && !down2 && press1 == now)
            {
                if (now - rel2 < _freq / 2)
                {
                    ms = (double)(press1 - rel2) * 1000.0 / _freq;
                    releaseKey = k2; pressKey = k1;
                    detected = true;
                }
            }
            // Overlap Mode: !down1 && down2 && rel1 == now
            else if (!down1 && down2 && rel1 == now)
            {
                if (now - press2 < _freq / 2)
                {
                    ms = (double)(press2 - rel1) * 1000.0 / _freq;
                    releaseKey = k1; pressKey = k2;
                    detected = true;
                }
            }
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
                return new StrafeResult { 
                    Axis = axis, 
                    Latency = ms, 
                    ReleaseKey = releaseKey, 
                    PressKey = pressKey,
                    StopTick = stopTick 
                };
            }
            return null;
        }
    }
}