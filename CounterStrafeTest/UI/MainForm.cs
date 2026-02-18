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
        
        // 用于测试功能的随机数生成器
        private Random _rng = new Random();

        private int _threshold = 120;
        private int _recCount = 40;

        public MainForm()
        {
            this.Text = "CS2 急停评估工具 Pro";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            
            // 绑定按钮事件，注意这里第一个参数是 OnTest
            LayoutBuilder.AddButtons(_ui, OnTest, OnMap, OnThreshold, OnCount, OnReset);

            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _strafeLogic = new StrafeLogic();
            _inputCore.Register(this.Handle);
            
            UpdateUiText();
        }

        private void ShowFeedbackBubble(string text, Color color)
        {
            // 清理旧气泡
            while (_ui.PnlBubbleArea.Controls.Count > 0)
            {
                Control old = _ui.PnlBubbleArea.Controls[0];
                _ui.PnlBubbleArea.Controls.Remove(old);
                old.Dispose();
            }
            
            var bubble = new BubbleControl(text, color);
            // 气泡位置
            bubble.Location = new Point(20, 20);
            
            bubble.AnimationComplete += (s, e) => {
                if (!_ui.PnlBubbleArea.IsDisposed && !bubble.IsDisposed)
                {
                    _ui.PnlBubbleArea.Controls.Remove(bubble);
                    bubble.Dispose();
                }
            };

            _ui.PnlBubbleArea.Controls.Add(bubble);
        }

        private void LogStrafeResult(StrafeResult r)
        {
            Color color = ColorHelper.GetColor(r.Latency);
            string evalRaw = ColorHelper.GetEvaluation(r.Latency);
            string evalText = (evalRaw == "Perfect" || evalRaw == "Great") ? evalRaw : Localization.Get("Eval_" + evalRaw);

            ShowFeedbackBubble($"{evalText}: {r.Latency:F1}ms", color);

            string msg = $"[{DateTime.Now:HH:mm:ss}] [{r.Axis}] {r.Latency:F1}ms ({evalText})";
            _ui.ListHistory.Items.Insert(0, msg);
            if (_ui.ListHistory.Items.Count > 100) _ui.ListHistory.Items.RemoveAt(100);

            if (r.Axis == "AD") _ui.GraphAD.AddValue((float)r.Latency);
            else _ui.GraphWS.AddValue((float)r.Latency);
        }

        private void OnReset(object s, EventArgs e)
        {
            if (MessageBox.Show("确认重置所有数据？", "重置", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _threshold = 120;
                _recCount = 40;
                _inputCore.ResetMapping();
                _ui.ListHistory.Items.Clear();
                _ui.GraphAD.Clear(); _ui.GraphAD.SetLimit(_recCount);
                _ui.GraphWS.Clear(); _ui.GraphWS.SetLimit(_recCount);
                _strafeLogic.Reset();
                _ui.PnlBubbleArea.Controls.Clear();
                MessageBox.Show("已重置。");
            }
        }

        private void UpdateUiText() 
        {
            // --- 【关键修复】 ---
            // 使用 BtnTest 而不是 BtnRefresh
            _ui.BtnTest.Text = Localization.Get("Btn_Test");
            // -------------------
            
            _ui.BtnReset.Text = Localization.Get("Btn_Reset");
            _ui.BtnMap.Text = Localization.Get("Btn_Mapping");
            _ui.BtnThreshold.Text = Localization.Get("Btn_Threshold");
            _ui.BtnCount.Text = Localization.Get("Btn_Count");
            _ui.GraphAD.SetTitle(Localization.Get("Chart_AD_Title"));
            _ui.GraphWS.SetTitle(Localization.Get("Chart_WS_Title"));
        }

        // --- 新增：测试按钮逻辑 ---
        private void OnTest(object s, EventArgs e)
        {
            string axis = _rng.Next(0, 2) == 0 ? "AD" : "WS";
            double latency = (_rng.NextDouble() * 20) - 10; 
            if (_rng.NextDouble() > 0.9) latency = (_rng.NextDouble() * 100) - 50;

            var result = new StrafeResult
            {
                Axis = axis,
                Latency = latency,
                ReleaseKey = axis == "AD" ? Keys.A : Keys.W,
                PressKey = axis == "AD" ? Keys.D : Keys.S
            };
            LogStrafeResult(result);
        }

        private void OnGameKey(object sender, GameKeyEventArgs e) {
             this.Invoke(new Action(() => UpdateKeyColor(e.LogicKey, e.IsDown)));
             var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
             if (result != null && result.IsValid && Math.Abs(result.Latency) <= _threshold) {
                 this.Invoke(new Action(() => LogStrafeResult(result)));
             }
        }
        
        private void UpdateKeyColor(Keys k, bool isDown) {
            Label t = k switch { Keys.W => _ui.LblW, Keys.A => _ui.LblA, Keys.S => _ui.LblS, Keys.D => _ui.LblD, _ => null };
            if (t != null) t.BackColor = isDown ? Color.LimeGreen : Color.LightGray;
        }
        
        protected override void WndProc(ref Message m) {
            if (m.Msg == NativeMethods.WM_INPUT) _inputCore.ProcessMessage(m);
            base.WndProc(ref m);
        }
        
        // 按钮事件占位
        private void OnMap(object s, EventArgs e) { 
             string val = InputBox.Show("Mapping", "Enter 4 letters (WASD):");
             if (!string.IsNullOrEmpty(val)) _inputCore.UpdateMapping(val.ToUpper());
        }
        private void OnThreshold(object s, EventArgs e) {
             string val = InputBox.Show("Threshold", "Enter ms:", _threshold.ToString());
             if (int.TryParse(val, out int v)) _threshold = v;
        }
        private void OnCount(object s, EventArgs e) {
             string val = InputBox.Show("Limit", "Count:", _recCount.ToString());
             if (int.TryParse(val, out int v)) { _recCount = v; _ui.GraphAD.SetLimit(v); _ui.GraphWS.SetLimit(v); }
        }
    }
}