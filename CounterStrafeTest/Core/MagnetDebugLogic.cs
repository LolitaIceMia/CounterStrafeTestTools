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
        
        // 算法给出的推荐值
        public float SuggestedRtActuation { get; set; }
        public float SuggestedRtReset { get; set; }
        public float SuggestedPressDeadzone { get; set; }
        public float SuggestedReleaseDeadzone { get; set; }
    }

    public class MagnetDebugLogic
    {
        private readonly List<double> _samples = new List<double>();
        private const int MAX_SAMPLES = 30;

        // === 当前键盘参数 (默认值，可由用户在界面修改) ===
        public float CurrentRtActuation { get; set; } = 0.5f;   // 快速触发行程
        public float CurrentRtReset { get; set; } = 0.5f;       // 快速重置行程
        public float CurrentPressDeadzone { get; set; } = 0.3f; // 死区按下触发
        public float CurrentReleaseDeadzone { get; set; } = 0.1f; // 死区抬起/迟滞

        public int SampleCount => _samples.Count;
        public int TargetSamples => MAX_SAMPLES;
        public bool IsReady => _samples.Count >= MAX_SAMPLES;

        public void AddSample(double latency)
        {
            // 过滤掉超过 200ms 的异常干扰值
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

            // 初始建议值设为当前值
            var result = new MagnetDebugResult
            {
                MeanLatency = mean,
                StdDev = stdDev,
                SuggestedRtActuation = CurrentRtActuation,
                SuggestedRtReset = CurrentRtReset,
                SuggestedPressDeadzone = CurrentPressDeadzone,
                SuggestedReleaseDeadzone = CurrentReleaseDeadzone
            };

            StringBuilder sb = new StringBuilder();

            // === 1. RT 行程分析 (基于均值 Mean) ===
            // 阈值设为 ±5ms
            if (mean < -5.0) 
            {
                // [急停过早]：说明松手判定太慢，建议减小重置行程
                // 每次优化步进 0.1mm，最低不低于 0.1mm
                result.SuggestedRtReset = Math.Max(0.1f, CurrentRtReset - 0.1f);
                
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Early")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Decrease_Reset")} ({CurrentRtReset:F1}mm -> {result.SuggestedRtReset:F1}mm)");
            }
            else if (mean > 5.0) 
            {
                // [急停过迟]：说明按下判定太慢，建议减小触发行程
                result.SuggestedRtActuation = Math.Max(0.1f, CurrentRtActuation - 0.1f);
                
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Late")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Decrease_Actuation")} ({CurrentRtActuation:F1}mm -> {result.SuggestedRtActuation:F1}mm)");
            }
            else
            {
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Good")}");
            }

            // === 2. 稳定性分析 (基于标准差 StdDev) ===
            // 阈值：8ms，抖动过大
            if (stdDev > 8.0)
            {
                // [颤噪/不稳定]：建议增加 抬起死区 (Release Deadzone / Hysteresis)
                // 步进 +0.05mm
                result.SuggestedReleaseDeadzone = Math.Min(1.0f, CurrentReleaseDeadzone + 0.05f);
                
                sb.AppendLine();
                sb.AppendLine($"• {Localization.Get("Mag_Analysis_Unstable")}");
                sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Increase_Deadzone")} ({CurrentReleaseDeadzone:F2}mm -> {result.SuggestedReleaseDeadzone:F2}mm)");
            }
            // === 3. 误触分析 (均值极负且方差大) ===
            else if (mean < -15.0 && stdDev > 10.0)
            {
                 // [严重误触]：建议增加 按下死区 (Press Deadzone)
                 result.SuggestedPressDeadzone = Math.Min(1.0f, CurrentPressDeadzone + 0.1f);

                 sb.AppendLine();
                 sb.AppendLine($"• {Localization.Get("Mag_Analysis_Mispress")}");
                 sb.AppendLine($"  -> {Localization.Get("Mag_Rec_Press_Deadzone")} ({CurrentPressDeadzone:F1}mm -> {result.SuggestedPressDeadzone:F1}mm)");
            }

            result.Recommendation = sb.ToString();
            return result;
        }
    }
}