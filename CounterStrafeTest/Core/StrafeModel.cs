using System;
using System.Windows.Forms; // 仅为了 Keys 枚举

namespace CounterStrafeTest.Core
{
    // 定义一次急停的数据结构
    public class StrafeRecord
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } 
        public double LatencyMs { get; set; }
        public Keys ReleaseKey { get; set; }
        public Keys PressKey { get; set; }
        public string Evaluation { get; set; } 
    }
}