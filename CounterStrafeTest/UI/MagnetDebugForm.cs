using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Core;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public class MagnetDebugForm : Form
    {
        private readonly MagnetDebugLogic _logic;
        private readonly InputCore _inputCore;
        private readonly StrafeLogic _strafeLogic;

        private Label _lblStatus;
        private ProgressBar _progressBar;
        private AdvancedGraph _graph;
        private Button _btnAction;
        private Button _btnSettings; // 设置参数按钮
        private bool _isTesting = false;

        public MagnetDebugForm()
        {
            _logic = new MagnetDebugLogic();
            _inputCore = new InputCore();
            _strafeLogic = new StrafeLogic();

            InitializeUi();

            // 注册输入监听
            _inputCore.OnGameKeyEvent += OnGameKey;
            _inputCore.Register(this.Handle);
        }

        private void InitializeUi()
        {
            this.Text = Localization.Get("Title_MagnetDebug");
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 主布局
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            // 定义行高：标题(60), 设置按钮(50), 图表(%), 进度条(40), 开始按钮(60)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); 

            // 1. 标题
            _lblStatus = new Label
            {
                Text = Localization.Get("Sim_Waiting"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gold
            };

            // 2. 设置按钮 (修复了之前的 Alignment 错误)
            _btnSettings = UiFactory.CreateButton("Btn_Magnet_Settings", Color.FromArgb(60, 60, 80));
            _btnSettings.Size = new Size(200, 35);
            // 关键修复：直接使用 Anchor.None 在 TableLayoutPanel 中居中
            _btnSettings.Anchor = AnchorStyles.None; 
            _btnSettings.Click += (s, e) => ShowSettingsDialog();

            // 3. 图表
            _graph = new AdvancedGraph { Dock = DockStyle.Fill };
            _graph.SetTitle(Localization.Get("Chart_AD_Title")); 
            _graph.SetLimit(30);

            // 4. 进度条
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Maximum = _logic.TargetSamples,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            // 5. 开始测试按钮
            _btnAction = UiFactory.CreateButton("Btn_Start_Test", null);
            _btnAction.Size = new Size(200, 40);
            _btnAction.Anchor = AnchorStyles.None;
            _btnAction.Click += OnActionClick;

            // 添加控件到布局
            layout.Controls.Add(_lblStatus, 0, 0);
            layout.Controls.Add(_btnSettings, 0, 1); // 直接添加按钮
            layout.Controls.Add(_graph, 0, 2);
            layout.Controls.Add(_progressBar, 0, 3);
            layout.Controls.Add(_btnAction, 0, 4);

            this.Controls.Add(layout);
        }

        private void ShowSettingsDialog()
        {
            var form = new Form
            {
                Text = Localization.Get("Dialog_Settings_Title"),
                Size = new Size(450, 350),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = UiFactory.ColorBack,
                ForeColor = UiFactory.ColorText,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };

            var pnl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 2,
                Padding = new Padding(20)
            };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            // 辅助方法：添加带标签的输入框
            void AddRow(string labelKey, float val, Action<float> setter)
            {
                var lbl = new Label 
                { 
                    Text = Localization.Get(labelKey), 
                    Dock = DockStyle.Fill, 
                    TextAlign = ContentAlignment.MiddleLeft, 
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10)
                };
                var txt = new TextBox 
                { 
                    Text = val.ToString("F2"), 
                    Dock = DockStyle.Fill, 
                    BackColor = Color.FromArgb(40,40,40), 
                    ForeColor = Color.White,
                    Font = new Font("Consolas", 10)
                };
                // 失去焦点时更新值
                txt.Leave += (s, e) => 
                { 
                    if (float.TryParse(txt.Text, out float v)) setter(v); 
                    else txt.Text = val.ToString("F2"); // 输入非法则回滚
                };
                pnl.Controls.Add(lbl);
                pnl.Controls.Add(txt);
            }

            // 添加 4 个参数行
            AddRow("Label_RtActuation", _logic.CurrentRtActuation, v => _logic.CurrentRtActuation = v);
            AddRow("Label_RtReset", _logic.CurrentRtReset, v => _logic.CurrentRtReset = v);
            AddRow("Label_PressDz", _logic.CurrentPressDeadzone, v => _logic.CurrentPressDeadzone = v);
            AddRow("Label_ReleaseDz", _logic.CurrentReleaseDeadzone, v => _logic.CurrentReleaseDeadzone = v);

            // 确认按钮
            var btnOk = UiFactory.CreateButton("OK", Color.LimeGreen);
            btnOk.Click += (s, e) => form.Close();
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            
            pnl.Controls.Add(btnOk, 1, 4);

            form.Controls.Add(pnl);
            form.ShowDialog(this);
        }

        private void OnActionClick(object sender, EventArgs e)
        {
            if (!_isTesting)
            {
                // 开始测试
                _isTesting = true;
                _logic.Reset();
                _strafeLogic.Reset();
                _graph.Clear();
                _progressBar.Value = 0;
                
                _btnAction.Text = Localization.Get("Btn_Stop");
                _btnAction.BackColor = UiFactory.ColorFail;
                _lblStatus.Text = $"Testing... 0/{_logic.TargetSamples}";
                _lblStatus.ForeColor = Color.White;
                _btnSettings.Enabled = false; // 测试过程中禁止修改设置
            }
            else
            {
                StopTest();
            }
        }

        private void StopTest()
        {
            _isTesting = false;
            _btnAction.Text = Localization.Get("Btn_Start_Test");
            _btnAction.BackColor = Color.FromArgb(50, 50, 50);
            _lblStatus.Text = Localization.Get("Sim_Waiting");
            _lblStatus.ForeColor = Color.Gold;
            _btnSettings.Enabled = true;
        }

        private void OnGameKey(object sender, GameKeyEventArgs e)
        {
            if (!_isTesting) return;

            this.Invoke(new Action(() =>
            {
                var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
                if (result != null && result.IsValid)
                {
                    _logic.AddSample(result.Latency);
                    _graph.AddValue((float)result.Latency);
                    _progressBar.Value = Math.Min(_logic.SampleCount, _progressBar.Maximum);
                    _lblStatus.Text = $"Samples: {_logic.SampleCount} / {_logic.TargetSamples}";

                    if (_logic.IsReady)
                    {
                        FinishTest();
                    }
                }
            }));
        }

        private void FinishTest()
        {
            StopTest();
            var result = _logic.Analyze();

            // 结果展示弹窗
            string msg = $"{Localization.Get("Mag_Report_Mean")}: {result.MeanLatency:F2} ms\n" +
                         $"{Localization.Get("Mag_Report_StdDev")}: {result.StdDev:F2}\n\n" +
                         $"{Localization.Get("Mag_Report_Rec")}\n" +
                         $"{result.Recommendation}\n\n" +
                         $"{Localization.Get("Mag_Report_Settings")}:\n" +
                         $"{Localization.Get("Label_RtActuation")}: {result.SuggestedRtActuation:F1} mm\n" +
                         $"{Localization.Get("Label_RtReset")}: {result.SuggestedRtReset:F1} mm\n" +
                         $"{Localization.Get("Label_PressDz")}: {result.SuggestedPressDeadzone:F1} mm\n" +
                         $"{Localization.Get("Label_ReleaseDz")}: {result.SuggestedReleaseDeadzone:F2} mm";

            MessageBox.Show(msg, Localization.Get("Magnet_Report_Title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            _lblStatus.Text = Localization.Get("Magnet_Report_Title");
            _lblStatus.ForeColor = Color.LimeGreen;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // InputCore 通常随主窗体生命周期，若有单独 Register 需注意释放
            // 但此处使用 this.Handle，随窗口销毁自动失效
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_INPUT) _inputCore.ProcessMessage(m);
            base.WndProc(ref m);
        }
    }
}