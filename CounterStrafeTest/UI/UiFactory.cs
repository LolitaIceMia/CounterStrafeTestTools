using System;
using System.Drawing;
using System.Windows.Forms;

namespace CounterStrafeTest.Utils
{
    public static class UiFactory
    {
        // 统一背景色配置
        public static readonly Color ColorBack = Color.FromArgb(30, 30, 30);
        public static readonly Color ColorPanel = Color.FromArgb(40, 40, 40);
        public static readonly Color ColorText = Color.White;

        public static Label CreateTitleLabel(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei", 24, FontStyle.Bold),
                Text = text,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = ColorText
            };
        }

        public static Label CreateKeyLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,             
                Size = new Size(50, 50),  
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 16, FontStyle.Bold), // 字体稍微加大一点
                Margin = new Padding(4) 
            };
        }

        public static Button CreateButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Width = 75,
                Height = 35,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup,
                Font = new Font("Microsoft YaHei", 9)
            };
            btn.Click += onClick;
            return btn;
        }

        public static ListBox CreateLogList()
        {
            return new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ColorPanel,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10),
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle
            };
        }
    }
}