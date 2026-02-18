using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CounterStrafeTest.UI
{
    public class BubbleControl : Control
    {
        private readonly string _text;
        private readonly Color _baseColor;
        private readonly Timer _timer;
        
        private int _alpha = 255;
        private int _lifeTimeTicks = 0;
        
        private const int HoldTicks = 60; 
        private const int FadeTicks = 12; 
        private const int CornerRadius = 10; // 调整圆角

        public event EventHandler AnimationComplete;

        public BubbleControl(string text, Color color)
        {
            _text = text;
            _baseColor = color;
            
            // --- 修改：尺寸调整 ---
            this.Size = new Size(200, 50); 
            // ---------------------

            this.DoubleBuffered = true;
            
            // 启用透明背景支持
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.BackColor = Color.Transparent; 
            
            _timer = new Timer { Interval = 16 };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        // ... OnTimerTick 保持不变 ...
        private void OnTimerTick(object sender, EventArgs e)
        {
            _lifeTimeTicks++;
            if (_lifeTimeTicks <= HoldTicks) { }
            else if (_lifeTimeTicks <= HoldTicks + FadeTicks)
            {
                float progress = (float)(_lifeTimeTicks - HoldTicks) / FadeTicks;
                _alpha = (int)(255 * (1.0f - progress));
                this.Invalidate();
            }
            else
            {
                _timer.Stop();
                _alpha = 0;
                AnimationComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color bgCol = Color.FromArgb(_alpha, _baseColor);
            Color txtCol = Color.FromArgb(_alpha, GetTextColor(_baseColor));

            Rectangle rect = this.ClientRectangle;
            rect.Width -= 1; rect.Height -= 1;
            
            using (GraphicsPath path = GetRoundedRect(rect, CornerRadius))
            using (Brush brush = new SolidBrush(bgCol))
            {
                g.FillPath(brush, path);
            }

            // 字体稍微改小一点以适配 50px 高度
            using (Font font = new Font("Microsoft YaHei", 12, FontStyle.Bold)) 
            using (Brush textBrush = new SolidBrush(txtCol))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(_text, font, textBrush, rect, sf);
            }
        }

        // ... GetRoundedRect, GetTextColor, Dispose, CreateParams, OnPaintBackground 保持不变 ...
        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Color GetTextColor(Color bg)
        {
            double brightness = bg.R * 0.299 + bg.G * 0.587 + bg.B * 0.114;
            return brightness > 140 ? Color.Black : Color.White;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer?.Dispose();
            base.Dispose(disposing);
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; 
                return cp;
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}