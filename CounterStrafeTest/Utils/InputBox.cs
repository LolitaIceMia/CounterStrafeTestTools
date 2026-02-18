using System.Windows.Forms;
using System.Drawing;

namespace CounterStrafeTest.UI
{
    public static class InputBox
    {
        public static string Show(string title, string prompt, string defaultValue = "")
        {
            Form form = new Form() { Width = 400, Height = 200, FormBorderStyle = FormBorderStyle.FixedDialog, Text = title, StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(40,40,40), ForeColor = Color.White };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 260, Width = 100, Top = 100, DialogResult = DialogResult.OK, BackColor = Color.Gray, FlatStyle = FlatStyle.Flat };
            
            form.Controls.Add(textBox);
            form.Controls.Add(confirmation);
            form.Controls.Add(textLabel);
            form.AcceptButton = confirmation;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}