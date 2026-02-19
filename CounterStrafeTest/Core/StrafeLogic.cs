using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CounterStrafeTest.Core
{
    public class StrafeResult
    {
        public string Axis { get; set; }        // "AD" or "WS"
        public double Latency { get; set; }     // ms, 负数为过早(Overlap)，正数为过迟(Gap)
        public Keys ReleaseKey { get; set; }    // 松开的键
        public Keys PressKey { get; set; }      // 按下的键
        public long StopTick { get; set; }      // 急停完成的时刻 (QPC Tick)
        
        // 判定有效性：过滤掉超过 200ms 的异常数据
        public bool IsValid => Math.Abs(Latency) < 200; 
    }

    internal class KeyState
    {
        public bool IsPressed;
        public long PressTick;   // QPC Tick
        // Python 代码中只记录了 press time，没有明确记录 release time，
        // 只有在 waiting_for_opposite_key 中才记录 release_time。
        // 我们这里也不需要常驻 release tick，按需记录即可。
    }

    internal class PendingState
    {
        public bool IsWaiting;
        public Keys KeyReleased; // 哪个键松开了
        public long ReleaseTick; // 松开的时间
    }

    public class StrafeLogic
    {
        private long _freq;
        public long Frequency => _freq;

        private readonly Dictionary<Keys, KeyState> _keys = new Dictionary<Keys, KeyState>();

        // AD 和 WS 分别维护一个等待状态，对应 Python 中的 self.waiting_for_opposite_key
        private PendingState _pendingAD = new PendingState();
        private PendingState _pendingWS = new PendingState();

        public StrafeLogic()
        {
            NativeMethods.QueryPerformanceFrequency(out _freq);
            
            _keys[Keys.W] = new KeyState();
            _keys[Keys.A] = new KeyState();
            _keys[Keys.S] = new KeyState();
            _keys[Keys.D] = new KeyState();
        }

        public void Reset()
        {
            foreach (var k in _keys.Values)
            {
                k.IsPressed = false;
                k.PressTick = 0;
            }
            _pendingAD.IsWaiting = false;
            _pendingWS.IsWaiting = false;
        }

        public StrafeResult ProcessLogicKey(Keys key, bool isDown)
        {
            if (!_keys.ContainsKey(key)) return null;

            NativeMethods.QueryPerformanceCounter(out long now);
            var state = _keys[key];

            // 1. 按下事件 (on_press)
            if (isDown)
            {
                if (state.IsPressed) return null; // 忽略重复按下
                state.IsPressed = true;
                state.PressTick = now;

                // 对应 Python: on_press -> 检查 waiting_for_opposite_key
                return CheckGapStrafe(key, now);
            }
            // 2. 松开事件 (on_release)
            else
            {
                if (!state.IsPressed) return null; // 忽略重复松开
                state.IsPressed = false;
                
                // 对应 Python: on_release -> process_key_event
                return CheckOverlapStrafe_Or_SetPending(key, now);
            }
        }

        // 对应 Python on_press 中的逻辑：检查是否构成了“按晚了”(Gap)
        private StrafeResult CheckGapStrafe(Keys currentKey, long now)
        {
            // 确定反向键和 Pending 状态
            Keys oppositeKey;
            string axis;
            PendingState pending;
            GetContext(currentKey, out oppositeKey, out axis, out pending);
            
            if (pending.IsWaiting && pending.KeyReleased == oppositeKey)
            {
                // 计算时间差: PressTime - ReleaseTime
                double ms = (double)(now - pending.ReleaseTick) * 1000.0 / _freq;

                // 清除等待状态
                pending.IsWaiting = false;

                return new StrafeResult
                {
                    Axis = axis,
                    Latency = ms, // 正数
                    ReleaseKey = oppositeKey,
                    PressKey = currentKey,
                    StopTick = now // 动作结束于按下当前键
                };
            }
            
            return null;
        }

        // 对应 Python process_key_event 中的逻辑
        private StrafeResult CheckOverlapStrafe_Or_SetPending(Keys currentKey, long now)
        {
            // 注意：这里 currentKey 是刚刚松开的键 (Key Released)
            Keys oppositeKey;
            string axis;
            PendingState pending;
            GetContext(currentKey, out oppositeKey, out axis, out pending);

            var oppositeState = _keys[oppositeKey];

            // Python: if key_state[opposite]['pressed']:
            if (oppositeState.IsPressed)
            {
                // 构成了“按早了”(Overlap)
                // 计算时间差: PressTime(Opposite) - ReleaseTime(Current)
                // Python: time_diff = key_state[opposite]['time'] - release_time
                double ms = (double)(oppositeState.PressTick - now) * 1000.0 / _freq;

                // 清除可能存在的 Pending 状态 (以防万一)
                pending.IsWaiting = false;

                return new StrafeResult
                {
                    Axis = axis,
                    Latency = ms, // 负数
                    ReleaseKey = currentKey,
                    PressKey = oppositeKey,
                    StopTick = now // 动作结束于松开当前键
                };
            }
            else
            {
                // Python: else: self.waiting_for_opposite_key = ...
                // 进入等待状态
                pending.IsWaiting = true;
                pending.KeyReleased = currentKey;
                pending.ReleaseTick = now;
                return null;
            }
        }

        // 辅助方法：获取上下文信息
        private void GetContext(Keys key, out Keys opposite, out string axis, out PendingState pending)
        {
            if (key == Keys.A) { opposite = Keys.D; axis = "AD"; pending = _pendingAD; }
            else if (key == Keys.D) { opposite = Keys.A; axis = "AD"; pending = _pendingAD; }
            else if (key == Keys.W) { opposite = Keys.S; axis = "WS"; pending = _pendingWS; }
            else if (key == Keys.S) { opposite = Keys.W; axis = "WS"; pending = _pendingWS; }
            else { opposite = Keys.None; axis = ""; pending = null; }
        }
    }
}