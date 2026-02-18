using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
// 明确指定 Timer
using Timer = System.Windows.Forms.Timer;

namespace CounterStrafeTest.UI
{
    public class BubbleControl : Control
    {
        private readonly string _text;
        private readonly Color _baseColor;
        private readonly Timer _timer;
        
        // 动画状态
        private int _alpha = 255;
        private int _lifeTimeTicks = 0;
        
        // 动画配置
        private const int HoldTicks = 60; // 约 1秒
        private const int FadeTicks = 12; // 约 200ms
        private const int CornerRadius = 16; 

        public event EventHandler AnimationComplete;

        public BubbleControl(string text, Color color)
        {
            _text = text;
            _baseColor = color;
            
            this.Size = new Size(400, 75);
            
            // --- 修复关键点：启用透明背景支持 ---
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 防止闪烁
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
            // 现在设置透明背景就是安全的了
            this.BackColor = Color.Transparent; 
            
            _timer = new Timer { Interval = 16 };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        // 下面的代码保持不变
        private void OnTimerTick(object sender, EventArgs e)
        {
            _lifeTimeTicks++;

            if (_lifeTimeTicks <= HoldTicks)
            {
                // wait
            }
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

            using (Font font = new Font("Microsoft YaHei", 18, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(txtCol))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(_text, font, textBrush, rect, sf);
            }
        }

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
        
        // 关键：为了支持透明，还需要重写 CreateParams（对于某些情况是必须的，WinForms 的怪癖）
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }
        
        // 并且要在 OnPaintBackground 中不做任何事，防止父级背景被擦除
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing
        }
    }
}