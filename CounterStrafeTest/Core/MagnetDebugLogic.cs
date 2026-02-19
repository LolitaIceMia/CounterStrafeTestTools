using System;
using System.Collections.Generic;
using System.Linq;

namespace CounterStrafeTest.Core
{
    public class MagnetDebugResult
    {
        public double MeanLatency { get; set; }
        public double StdDev { get; set; }
        public string Recommendation { get; set; }
        public float SuggestedActuation { get; set; } // 推荐触发行程 (mm)
        public float SuggestedReset { get; set; }     // 推荐重置行程 (mm)
        public float SuggestedDeadzone { get; set; }  // 推荐死区 (mm)
    }

    public class MagnetDebugLogic
    {
        private readonly List<double> _samples = new List<double>();
        private const int MAX_SAMPLES = 30;

        // 当前键盘的假设默认值 (用于基于此进行微调)
        private float _currentActuation = 1.0f; 
        private float _currentReset = 0.5f;
        private float _currentDeadzone = 0.1f;

        public int SampleCount => _samples.Count;
        public int TargetSamples => MAX_SAMPLES;
        public bool IsReady => _samples.Count >= MAX_SAMPLES;

        public void AddSample(double latency)
        {
            if (Math.Abs(latency) < 200) // 过滤异常值
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

            List<string> tips = new List<string>();

            // 1. 分析均值 (偏向性)
            if (mean < -5.0) // 显著过早 (Overlap)
            {
                // 此时松开原键太慢，或按下新键太快
                // 策略：让松键更快生效 -> 减小重置行程
                result.SuggestedReset = Math.Max(0.1f, _currentReset - 0.2f);
                tips.Add("急停倾向过早：建议减小快速重置行程 (Rapid Trigger Reset)，加快松键响应。");
            }
            else if (mean > 5.0) // 显著过迟 (Gap)
            {
                // 此时按下新键太慢
                // 策略：让按键更快触发 -> 减小触发行程
                result.SuggestedActuation = Math.Max(0.1f, _currentActuation - 0.2f);
                tips.Add("急停倾向过迟：建议减小快速触发行程 (Rapid Trigger Actuation)。");
            }
            else
            {
                tips.Add("急停时机控制良好，保持当前行程设置。");
            }

            // 2. 分析方差 (稳定性)
            if (stdDev > 8.0)
            {
                // 操作不稳定，容易误触
                result.SuggestedDeadzone = Math.Min(0.5f, _currentDeadzone + 0.1f);
                tips.Add("操作波动较大：建议增加死区 (Deadzone) 以过滤手指抖动。");
            }

            result.Recommendation = string.Join("\n", tips);
            return result;
        }
    }
}