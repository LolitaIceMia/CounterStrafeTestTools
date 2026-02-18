using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Core;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public partial class MainForm : Form
    {
        // 组件引用
        private GameUiComponents _ui;
        
        // 核心模块
        private readonly InputCore _inputCore;
        private readonly StrafeLogic _strafeLogic;

        // 设置参数
        private int _threshold = 120;
        private int _recCount = 40;

        public MainForm()
        {
            // 1. 基础窗体属性
            this.Text = "CS2急停评估工具 Pro (Decoupled)";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            // 2. 构建界面 (调用 UI 包)
            _ui = LayoutBuilder.Build(this);
            
            // 绑定按钮事件 (通过查找控件或传入)
            // 获取 Button 面板比较麻烦，我们在 LayoutBuilder 中留了个 helper
            var leftPanel = (Panel)_ui.ListHistory.Parent;
            var btnPanel = (FlowLayoutPanel)leftPanel.Controls[1]; // 索引依赖于 LayoutBuilder 添加顺序：List(0), Btn(1), Grid(2) -> 实际是反的因为 Dock Fill
            // 修正：Dock Fill 的控件在 Controls 集合中通常索引较小，但为了稳妥，我们在 LayoutBuilder 加了 AddButtons
            LayoutBuilder.AddButtons(_ui, btnPanel, OnRefresh, OnMap, OnThreshold, OnCount);

            // 3. 初始化核心 (调用 Core 包)
            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey; // 订阅输入事件
            _strafeLogic = new StrafeLogic();

            // 4. 注册 RawInput
            _inputCore.Register(this.Handle);
        }

        // === 逻辑事件处理 ===

        private void OnGameKey(object sender, GameKeyEventArgs e)
        {
            // A. 更新按键灯效 (View)
            this.Invoke(new Action(() => UpdateKeyColor(e.LogicKey, e.IsDown)));

            // B. 运行急停算法 (Logic)
            var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);

            // C. 处理结果 (View Update)
            if (result != null && result.IsValid && Math.Abs(result.Latency) <= _threshold)
            {
                this.Invoke(new Action(() => LogStrafeResult(result)));
            }
        }

        private void LogStrafeResult(StrafeResult r)
        {
            // 1. 评价
            string eval = Localization.Get("Eval_Perfect");
            Color color = Color.LimeGreen;
            if (r.Latency < 0) { eval = Localization.Get("Eval_Early"); color = Color.DeepSkyBlue; }
            else if (r.Latency > 20) { eval = Localization.Get("Eval_Late"); color = Color.OrangeRed; }

            // 2. 更新日志和反馈
            string msg = $"[{DateTime.Now:HH:mm:ss}] [{r.Axis}] {r.Latency:F1}ms ({eval})";
            _ui.ListHistory.Items.Insert(0, msg);
            if (_ui.ListHistory.Items.Count > 100) _ui.ListHistory.Items.RemoveAt(100);

            _ui.LblFeedback.Text = msg;
            _ui.LblFeedback.BackColor = color;

            // 3. 更新图表
            if (r.Axis == "AD") _ui.GraphAD.AddValue((float)r.Latency);
            else _ui.GraphWS.AddValue((float)r.Latency);
        }

        private void UpdateKeyColor(Keys k, bool isDown)
        {
            Label target = k switch { Keys.W => _ui.LblW, Keys.A => _ui.LblA, Keys.S => _ui.LblS, Keys.D => _ui.LblD, _ => null };
            if (target != null)
            {
                target.BackColor = isDown ? Color.LimeGreen : Color.LightGray;
                target.ForeColor = isDown ? Color.Black : Color.Black;
            }
        }

        // === 消息循环 (仅仅为了传递给 InputCore) ===
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_INPUT)
            {
                _inputCore.ProcessMessage(m); // 委托给 Core 处理
            }
            base.WndProc(ref m);
        }

        // === 按钮事件 ===
        private void OnRefresh(object s, EventArgs e) 
        { 
            _ui.ListHistory.Items.Clear(); 
            _ui.GraphAD.Clear(); _ui.GraphWS.Clear(); 
            _strafeLogic.Reset();
            _ui.LblFeedback.Text = "Ready";
            _ui.LblFeedback.BackColor = Color.FromArgb(50,50,50);
        }
        
        private void OnMap(object s, EventArgs e)
        {
            string val = InputBox.Show("Mapping", "Enter 4 letters (WASD):");
            if (!string.IsNullOrEmpty(val)) _inputCore.UpdateMapping(val.ToUpper());
        }

        private void OnThreshold(object s, EventArgs e)
        {
            string val = InputBox.Show("Threshold", "Enter ms:", _threshold.ToString());
            if (int.TryParse(val, out int v)) _threshold = v;
        }

        private void OnCount(object s, EventArgs e)
        {
            string val = InputBox.Show("Limit", "Count:", _recCount.ToString());
            if (int.TryParse(val, out int v)) { _recCount = v; _ui.GraphAD.SetLimit(v); _ui.GraphWS.SetLimit(v); }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5) { OnRefresh(null, null); return true; }
            if (keyData == Keys.F6) { OnMap(null, null); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}