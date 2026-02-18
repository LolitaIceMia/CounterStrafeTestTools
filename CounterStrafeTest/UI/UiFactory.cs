using System.Drawing;
using System.Windows.Forms;

namespace CounterStrafeTest.Utils
{
    public static class UiFactory
    {
        // 定义全局使用的颜色
        public static readonly Color ColorBack = Color.FromArgb(18, 18, 18);
        public static readonly Color ColorText = Color.FromArgb(220, 220, 220);
        
        // [修复] 添加 ColorFail 定义，用于重置按钮的红色背景
        public static readonly Color ColorFail = Color.FromArgb(180, 60, 60); 

        // 创建按键显示标签 (WASD)
        public static Label CreateKeyLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Margin = new Padding(4)
            };
        }

        // [重点修复] 方法签名修改为接收 (string, Color?)
        // 之前可能是 (string, EventHandler)，导致传入 ColorFail 时报错
        public static Button CreateButton(string textKey, Color? bgColor = null)
        {
            return new Button
            {
                // 使用 Localization 获取文本，如果 textKey 为 null 则显示空
                Text = Localization.Get(textKey),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                // 如果传入了 bgColor 则使用，否则使用默认深灰色
                BackColor = bgColor ?? Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
        }
    }
}