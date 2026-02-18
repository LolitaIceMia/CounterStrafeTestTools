using System;
using System.Windows.Forms;
using CounterStrafeTest.UI;

namespace CounterStrafeTest
{
    internal static class Program
    {
        // 全局静态属性，存储当前是否为磁轴模式
        public static bool IsMagneticMode { get; private set; } = false;

        [STAThread]
        static void Main()
        {
            // 初始化应用配置 (支持高DPI等)
            ApplicationConfiguration.Initialize();

            // 1. 启动时弹出询问窗口
            // 使用 Localization.Get 获取多语言文本
            DialogResult result = MessageBox.Show(
                Localization.Get("Startup_Msg"),
                Localization.Get("Startup_Title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // 2. 记录用户的选择状态
            IsMagneticMode = (result == DialogResult.Yes);

            // 3. 将选择结果 (bool) 传递给 MainForm 构造函数
            Application.Run(new MainForm(IsMagneticMode));
        }
    }
}