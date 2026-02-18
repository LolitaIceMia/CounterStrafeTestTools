using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    // 必须继承自 Form，否则 Mainform 中无法调用 .ShowDialog()
    public class MagnetDebugForm : Form
    {
        public MagnetDebugForm()
        {
            // 基础窗体设置
            this.Text = Localization.Get("Title_MagnetDebug"); // 确保 Localization 中有此 Key，否则显示 Key 名
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 标题栏
            Label lblTitle = new Label
            {
                Text = "磁轴性能分析与校准 (开发中)",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Gold
            };

            // 占位内容提示
            Label lblContent = new Label
            {
                Text = "此处将显示 RT 曲线与方差统计图表...\n\n(功能正在开发中)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray
            };

            this.Controls.Add(lblContent);
            this.Controls.Add(lblTitle);
        }
    }
}