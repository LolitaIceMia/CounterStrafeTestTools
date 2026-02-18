using System;
using System.Drawing;

namespace CounterStrafeTest.Utils
{
    public static class ColorHelper
    {
        // 预定义颜色
        public static readonly Color ColGold = Color.FromArgb(255, 215, 0);       // ±1ms 金色
        public static readonly Color ColGreen = Color.FromArgb(50, 205, 50);      // ±5ms 绿色
        
        // 渐变端点
        private static readonly Color ColEarlyStart = Color.FromArgb(135, 206, 250); // LightSkyBlue (粉蓝)
        private static readonly Color ColEarlyEnd = Color.FromArgb(0, 0, 139);       // DarkBlue (深蓝)
        
        private static readonly Color ColLateStart = Color.FromArgb(255, 182, 193);  // LightPink (粉红)
        private static readonly Color ColLateEnd = Color.FromArgb(139, 0, 0);        // DarkRed (深红)

        // 渐变范围 (超过 60ms 就显示最深色)
        private const double GradientRange = 60.0; 

        public static Color GetColor(double ms)
        {
            double absMs = Math.Abs(ms);

            // 1. 完美区间 (±1ms)
            if (absMs <= 1.0) return ColGold;

            // 2. 优秀区间 (±5ms)
            if (absMs <= 5.0) return ColGreen;

            // 3. 渐变区间
            // 计算渐变比例 (从 5ms 开始计算，到 65ms 达到 100%)
            double ratio = (absMs - 5.0) / GradientRange;
            if (ratio > 1.0) ratio = 1.0;

            if (ms < 0) // 早 (负数)
            {
                return Interpolate(ColEarlyStart, ColEarlyEnd, ratio);
            }
            else // 晚 (正数)
            {
                return Interpolate(ColLateStart, ColLateEnd, ratio);
            }
        }

        public static string GetEvaluation(double ms)
        {
            double absMs = Math.Abs(ms);
            if (absMs <= 1.0) return "Perfect";
            if (absMs <= 5.0) return "Great";
            return ms < 0 ? "Early" : "Late";
        }

        // 线性颜色插值
        private static Color Interpolate(Color start, Color end, double ratio)
        {
            int r = (int)(start.R + (end.R - start.R) * ratio);
            int g = (int)(start.G + (end.G - start.G) * ratio);
            int b = (int)(start.B + (end.B - start.B) * ratio);
            return Color.FromArgb(r, g, b);
        }
    }
}