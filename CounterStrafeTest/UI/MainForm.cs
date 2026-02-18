using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Core;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public partial class MainForm : Form
    {
        private GameUiComponents _ui;
        private readonly InputCore _inputCore;
        private readonly StrafeLogic _strafeLogic;

        // 默认值常量
        private const int DefaultThreshold = 120;
        private const int DefaultRecCount = 40;

        private int _threshold = DefaultThreshold;
        private int _recCount = DefaultRecCount;

        public MainForm()
        {
            this.Text = "CS2 急停评估工具 Pro (Final)";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            
            // 获取 Button 面板 (LeftPanel -> BottomControl)
            // 更加稳健的获取方式：
            var contentLayout = (TableLayoutPanel)this.Controls[0].Controls[1]; // MainLayout -> ContentLayout
            var leftPanel = (Panel)contentLayout.Controls[0];
            // 按照 LayoutBuilder 的添加顺序: List(0), Btn(1), KeyContainer(2) -> 因为 Dock 顺序反向
            // Fill 是 0 (ListHistory), Bottom 是 1 (LayoutPanel), Top 是 2 (KeyContainer)
            // 实际上 WinForms Controls 集合顺序通常是后添加的在前面(Z-order)，但对于 Dock 来说不一定
            // 最安全的方式是直接遍历找 FlowLayoutPanel
            FlowLayoutPanel btnPanel = null;
            foreach(Control c in leftPanel.Controls) {
                if (c is FlowLayoutPanel) { btnPanel = (FlowLayoutPanel)c; break; }
            }

            if (btnPanel != null)
                LayoutBuilder.AddButtons(_ui, btnPanel, OnRefresh, OnMap, OnThreshold, OnCount, OnReset);

            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _strafeLogic = new StrafeLogic();

            _inputCore.Register(this.Handle);
            
            // 初始化文本
            UpdateUiText();
        }

        // ... (OnGameKey, LogStrafeResult, UpdateKeyColor, WndProc 保持不变) ...
        // ... 请确保 UpdateKeyColor 方法里映射是正确的 ...

        // === 按钮事件 ===

        private void OnReset(object s, EventArgs e)
        {
            if (MessageBox.Show("确定要重置所有设置和数据吗？", "重置", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // 1. 重置参数
                _threshold = DefaultThreshold;
                _recCount = DefaultRecCount;

                // 2. 重置映射
                _inputCore.ResetMapping();

                // 3. 重置数据和状态
                _ui.ListHistory.Items.Clear();
                _ui.GraphAD.Clear();
                _ui.GraphAD.SetLimit(_recCount);
                _ui.GraphWS.Clear();
                _ui.GraphWS.SetLimit(_recCount);
                _strafeLogic.Reset();

                // 4. 重置 UI 反馈
                _ui.LblFeedback.Text = "Ready";
                _ui.LblFeedback.BackColor = Color.FromArgb(50, 50, 50);

                MessageBox.Show("已恢复默认设置。", "完成");
            }
        }

        private void UpdateUiText()
        {
            // 这里可以统一更新按钮文字，支持多语言
            _ui.BtnRefresh.Text = Localization.Get("Btn_Refresh");
            _ui.BtnMap.Text = Localization.Get("Btn_Mapping");
            _ui.BtnThreshold.Text = Localization.Get("Btn_Threshold");
            _ui.BtnCount.Text = Localization.Get("Btn_Count");
            _ui.BtnReset.Text = Localization.Get("Btn_Reset");
            
            _ui.GraphAD.SetTitle(Localization.Get("Chart_AD_Title"));
            _ui.GraphWS.SetTitle(Localization.Get("Chart_WS_Title"));
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