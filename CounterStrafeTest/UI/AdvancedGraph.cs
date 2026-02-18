using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CounterStrafeTest.UI
{
    public class AdvancedGraph : Control
    {
        // === 数据源 ===
        private readonly List<float> _dataBuffer = new List<float>();
        private string _title = "Chart";
        private int _limit = 20; // 显示最近多少次
        
        // === 布局配置 ===
        private const int PadLeft = 50;   // 左边距（给Y轴文字）
        private const int PadBottom = 30; // 下边距（给X轴文字）
        private const int PadTop = 30;    // 标题
        private const int PadRight = 10;  
        private const int DistBarWidth = 20; // 右侧分布条的宽度

        public AdvancedGraph()
        {
            this.DoubleBuffered = true; // 防止闪烁
            this.BackColor = Color.FromArgb(20, 20, 20); // 深灰背景
            this.ForeColor = Color.White;
            this.ResizeRedraw = true; // 关键：改变大小时自动重绘
        }

        public void SetTitle(string title) { _title = title; Invalidate(); }
        public void SetLimit(int limit) { _limit = limit; Invalidate(); }

        // 接收新数据
        public void AddValue(float ms)
        {
            _dataBuffer.Add(ms);
            Invalidate(); // 触发重绘
        }

        public void Clear()
        {
            _dataBuffer.Clear();
            Invalidate();
        }

        // === 核心绘制逻辑 (View) ===
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float w = this.Width;
            float h = this.Height;
            float graphW = w - PadLeft - PadRight;
            float graphH = h - PadTop - PadBottom;
            float zeroY = PadTop + graphH / 2; // 0ms 线在中间

            // 1. 绘制边框和背景
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60)))
            {
                g.DrawRectangle(borderPen, PadLeft, PadTop, graphW, graphH);
            }

            // 2. 绘制 Y 轴 (时间差)
            DrawYAxis(g, zeroY, graphW, graphH);

            // 3. 绘制标题
            float avg = _dataBuffer.Count > 0 ? _dataBuffer.TakeLast(_limit).Average() : 0f;
            string titleFull = $"{_title} - Avg: {avg:F1}ms";
            using (Font titleFont = new Font("Microsoft YaHei", 10, FontStyle.Bold))
            {
                SizeF size = g.MeasureString(titleFull, titleFont);
                g.DrawString(titleFull, titleFont, Brushes.White, (w - size.Width) / 2, 5);
            }

            // 如果没数据，直接返回
            var drawData = _dataBuffer.TakeLast(_limit).ToList();
            if (drawData.Count == 0) return;

            // 4. 绘制 X 轴 (次数)
            DrawXAxis(g, drawData.Count, graphW, h);

            // 5. 绘制散点图
            DrawScatterPlot(g, drawData, graphW, graphH, zeroY);

            // 6. 绘制右侧分布条 (简易箱线图)
            // DrawDistribution(g, drawData, w - PadRight - DistBarWidth, PadTop, DistBarWidth, graphH);
        }

        private void DrawYAxis(Graphics g, float zeroY, float graphW, float graphH)
        {
            using (Pen axisPen = new Pen(Color.Gray) { DashStyle = DashStyle.Dot })
            using (Font font = new Font("Arial", 8))
            using (Brush brush = new SolidBrush(Color.Silver))
            {
                // 0ms 线
                g.DrawLine(Pens.White, PadLeft, zeroY, PadLeft + graphW, zeroY);
                g.DrawString("0", font, brush, 5, zeroY - 6);

                // +50ms 线
                float yPos50 = zeroY - (graphH / 2) * (50f / 60f); // 假设量程60ms
                g.DrawLine(axisPen, PadLeft, yPos50, PadLeft + graphW, yPos50);
                g.DrawString("+50", font, brush, 5, yPos50 - 6);

                // -50ms 线
                float yNeg50 = zeroY + (graphH / 2) * (50f / 60f);
                g.DrawLine(axisPen, PadLeft, yNeg50, PadLeft + graphW, yNeg50);
                g.DrawString("-50", font, brush, 5, yNeg50 - 6);
            }
        }

        private void DrawXAxis(Graphics g, int count, float graphW, float h)
        {
            using (Font font = new Font("Arial", 8))
            {
                g.DrawString("Count ->", font, Brushes.Gray, PadLeft + graphW - 50, h - 20);
            }
        }

        private void DrawScatterPlot(Graphics g, List<float> data, float graphW, float graphH, float zeroY)
        {
            float xStep = graphW / Math.Max(1, _limit - 1);
            float yRange = 60f; // Y轴量程 +/- 60ms

            for (int i = 0; i < data.Count; i++)
            {
                float val = data[i];
                // X坐标：从左到右分布
                float x = PadLeft + i * xStep;
                
                // Y坐标：映射值到像素高度
                float normalizedVal = Math.Clamp(val, -yRange, yRange);
                float pxOffset = (normalizedVal / yRange) * (graphH / 2);
                float y = zeroY - pxOffset; // WinForms Y轴向下是正，所以减去偏移量

                // 颜色判定
                Color c = Color.Gray;
                if (Math.Abs(val) <= 20) c = Color.LimeGreen;
                else if (val < 0) c = Color.DeepSkyBlue; // 早
                else c = Color.OrangeRed; // 晚

                using (Brush b = new SolidBrush(c))
                {
                    g.FillEllipse(b, x - 4, y - 4, 8, 8); // 画点
                }
            }
        }
    }
}