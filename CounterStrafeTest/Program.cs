using System;
using System.Windows.Forms;
using CounterStrafeTest.UI;

namespace CounterStrafeTest
{
    internal static class Program
    {
        public static bool IsMagneticMode { get; private set; } = false;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Localization.CurrentLanguage = AppLanguage.English; 

            DialogResult result = MessageBox.Show(
                Localization.Get("Startup_Msg"),
                Localization.Get("Startup_Title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            IsMagneticMode = (result == DialogResult.Yes);

            Application.Run(new MainForm());
        }
    }
}