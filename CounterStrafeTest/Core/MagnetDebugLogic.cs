using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CounterStrafeTest.Core
{
    public class MagnetDebugResult
    {
        public double MeanLatency { get; set; }
        public double StdDev { get; set; }
        public string Recommendation { get; set; }
        public float SuggestedActuation { get; set; } // 推荐触发行程 (AP)
        public float SuggestedReset { get; set; }     // 推荐重置行程 (RT)
        public float SuggestedDeadzone { get; set; }  // 推荐死区 (Deadzone)
    }

    public class MagnetDebugLogic
    {
        private readonly List<double> _samples = new List<double>();
        private const int MAX_SAMPLES = 30;

        // 当前键盘设置 (实际应用中应从驱动或配置文件读取)
        private float _currentActuation = 1.0f; 
        private float _currentReset = 0.5f;
        private float _currentDeadzone = 0.1f;
        public int SampleCount => _samples.Count;
        public int TargetSamples => MAX_SAMPLES;
        public bool IsReady => _samples.Count >= MAX_SAMPLES;

        public void AddSample(double latency)
        {
            // 过滤掉超过 200ms 的异常值，防止干扰统计
            if (Math.Abs(latency) < 200) 
            {
                _samples.Add(latency);
            }
        }

        public void Reset()
        {
            _samples.Clear();
        }

        public MagnetDebugResult Analyze()
        {
            if (_samples.Count == 0) return null;

            double mean = _samples.Average();
            double sumSquares = _samples.Sum(d => Math.Pow(d - mean, 2));
            double stdDev = Math.Sqrt(sumSquares / _samples.Count);

            var result = new MagnetDebugResult
            {
                MeanLatency = mean,
                StdDev = stdDev,
                SuggestedActuation = _currentActuation,
                SuggestedReset = _currentReset,
                SuggestedDeadzone = _currentDeadzone
            };

            StringBuilder sb = new StringBuilder();

            // === 1. RT 行程分析 (基于均值 Mean) ===
            // 均值反映了操作的系统性偏差
            if (mean < -5.0) 
            {
                // [急停过早]：松键太慢 或 反向键触发太快
                // 建议：减小重置行程，让松手判定更灵敏
                result.SuggestedReset = Math.Max(0.1f, _currentReset - 0.2f);
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Early")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Decrease_Reset")}");
            }
            else if (mean > 5.0) 
            {
                // [急停过迟]：反向键触发太慢
                // 建议：减小触发行程，让按下判定更灵敏
                result.SuggestedActuation = Math.Max(0.1f, _currentActuation - 0.2f);
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Late")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Decrease_Actuation")}");
            }
            else
            {
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Good")}");
            }

            // === 2. 死区设置分析 (基于标准差 StdDev) ===
            // CSV 参考: "RT 过程 (RT Mode), 信号颤噪 (Chatter), Setting + 0.05mm 动态迟滞"
            // 标准差反映了手指的抖动程度和传感器的噪声影响
            
            // 阈值设定：8ms (经验值，超过此值说明操作不稳定)
            if (stdDev > 8.0)
            {
                // [稳定性差/颤噪]
                // 建议：增加 "稳定性税" (Stability Tax)，即动态死区
                result.SuggestedDeadzone = Math.Min(0.5f, _currentDeadzone + 0.05f); 
                
                sb.AppendLine();
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Unstable")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Increase_Deadzone")} (+0.05mm)");
            }
            // 极端情况：如果均值极小 (如 -15ms) 且方差也大，可能是按下阶段误触
            // CSV 参考: "按下 (Press), 误触... 0.3mm - 0.5mm 固定偏移"
            else if (mean < -15.0 && stdDev > 10.0)
            {
                 // 建议：检查初始死区 (Press Deadzone)
                 sb.AppendLine();
                 sb.AppendLine($"• {Localization.Get("Mag_Analysis_Mispress")}");
                 sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Press_Deadzone")}");
            }

            result.Recommendation = sb.ToString();
            return result;
        }
    }
}