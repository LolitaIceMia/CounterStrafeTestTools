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
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // 标题
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // 图表
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // 进度条
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // 按钮

            // 1. 标题/状态
            _lblStatus = new Label
            {
                Text = "点击下方按钮开始测试",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gold
            };

            // 2. 图表
            _graph = new AdvancedGraph { Dock = DockStyle.Fill };
            _graph.SetTitle("实时急停延迟分布 (ms)");
            _graph.SetLimit(30);

            // 3. 进度条
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Maximum = _logic.TargetSamples,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            // 4. 控制按钮
            _btnAction = UiFactory.CreateButton("Btn_Start_Test", null); // 需要在 Localization 添加此 Key
            _btnAction.Text = "开始测试并调节"; // 临时文本，建议用 Localization
            _btnAction.Size = new Size(200, 40);
            _btnAction.Anchor = AnchorStyles.None;
            _btnAction.Click += OnActionClick;

            layout.Controls.Add(_lblStatus, 0, 0);
            layout.Controls.Add(_graph, 0, 1);
            layout.Controls.Add(_progressBar, 0, 2);
            layout.Controls.Add(_btnAction, 0, 3);

            this.Controls.Add(layout);
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
                
                _btnAction.Text = "停止测试";
                _btnAction.BackColor = UiFactory.ColorFail;
                _lblStatus.Text = $"请进行 {_logic.TargetSamples} 次急停操作 (AD/WS)...";
                _lblStatus.ForeColor = Color.White;
            }
            else
            {
                // 停止测试
                StopTest();
            }
        }

        private void StopTest()
        {
            _isTesting = false;
            _btnAction.Text = "重新开始测试";
            _btnAction.BackColor = Color.FromArgb(50, 50, 50);
            _lblStatus.Text = "测试已停止";
        }

        private void OnGameKey(object sender, GameKeyEventArgs e)
        {
            if (!_isTesting) return;

            // 必须在 UI 线程处理
            this.Invoke(new Action(() =>
            {
                var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
                if (result != null && result.IsValid)
                {
                    // 记录数据
                    _logic.AddSample(result.Latency);
                    
                    // 更新 UI
                    _graph.AddValue((float)result.Latency);
                    _progressBar.Value = Math.Min(_logic.SampleCount, _progressBar.Maximum);
                    _lblStatus.Text = $"进度: {_logic.SampleCount} / {_logic.TargetSamples}";

                    // 检查是否完成
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

            string msg = $"测试完成！\n\n" +
                         $"平均延迟: {result.MeanLatency:F2} ms\n" +
                         $"稳定性(标准差): {result.StdDev:F2}\n\n" +
                         $"--- 调节建议 ---\n" +
                         $"{result.Recommendation}\n\n" +
                         $"推荐设置:\n" +
                         $"触发行程: {result.SuggestedActuation:F1} mm\n" +
                         $"重置行程: {result.SuggestedReset:F1} mm\n" +
                         $"死区: {result.SuggestedDeadzone:F1} mm";

            MessageBox.Show(msg, "磁轴校准报告", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _lblStatus.Text = "校准完成，请参考弹窗建议调整驱动";
            _lblStatus.ForeColor = Color.LimeGreen;
        }

        // 确保窗口关闭时注销钩子，防止内存泄漏或异常
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // 这里 InputCore 没有 Unregister 方法，但随窗口销毁通常句柄会失效
            // 如果 InputCore 是全局单例需手动处理，这里是实例，GC 会回收
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_INPUT) _inputCore.ProcessMessage(m);
            base.WndProc(ref m);
        }
    }
}