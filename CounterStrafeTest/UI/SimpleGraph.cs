using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CounterStrafeTest.UI
{
    // 一个轻量级的绘图控件，用于替代 Matplotlib 绘制急停分布图
    public class SimpleGraph : Control
    {
        private List<float> _dataPoints = new List<float>();
        private string _title = "Chart";
        private float _average = 0f;
        
        public SimpleGraph()
        {
            this.DoubleBuffered = true; // 开启双缓冲，防止闪烁
            this.BackColor = Color.FromArgb(40, 40, 40); // 深色背景
            this.ForeColor = Color.White;
        }

        public void SetTitle(string title) { _title = title; Invalidate(); }

        public void AddPoint(float valueMs)
        {
            _dataPoints.Add(valueMs);
            if (_dataPoints.Count > 50) _dataPoints.RemoveAt(0); // 保持最近50个点
            _average = _dataPoints.Average();
            this.Invalidate(); // 触发重绘
        }
        
        public void Clear() { _dataPoints.Clear(); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Width;
            int h = Height;
            int padding = 30;

            // 1. 绘制坐标轴
            using (Pen axisPen = new Pen(Color.Gray, 1))
            {
                g.DrawLine(axisPen, padding, h / 2, w - padding, h / 2); // X轴 (0ms线)
                g.DrawLine(axisPen, padding, padding, padding, h - padding); // Y轴
            }

            // 绘制标题
            g.DrawString($"{_title} (Avg: {_average:F1}ms)", this.Font, Brushes.LightGray, padding, 5);

            if (_dataPoints.Count < 2) return;

            // 2. 绘制数据点
            // Y轴范围: -50ms 到 +50ms (超出截断)
            float yRange = 50f; 
            float xStep = (float)(w - 2 * padding) / 50f; // 固定显示50个点的宽度

            for (int i = 0; i < _dataPoints.Count; i++)
            {
                float val = _dataPoints[i];
                float x = padding + i * xStep + (50 - _dataPoints.Count) * xStep; // 靠右对齐
                
                // 映射 Y 坐标: 中心是 0
                float normalizedY = Math.Clamp(val, -yRange, yRange) / yRange; 
                float y = (h / 2) - (normalizedY * (h / 2 - padding));

                // 根据延迟决定颜色
                Brush pointBrush = Brushes.LightGray;
                if (Math.Abs(val) <= 20) pointBrush = Brushes.LimeGreen; // 完美
                else if (val < 0) pointBrush = Brushes.Cyan; // 早了
                else pointBrush = Brushes.OrangeRed; // 晚了

                g.FillEllipse(pointBrush, x - 3, y - 3, 6, 6);
            }
        }
    }
}