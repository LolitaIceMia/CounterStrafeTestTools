using System;
using System.Collections.Generic;
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

        // 气泡管理列表：Index 0 始终是最新的
        private List<BubbleControl> _activeBubbles = new List<BubbleControl>();

        private int _threshold = 120;
        private int _recCount = 40;

        public MainForm()
        {
            this.Text = "CS2 急停评估工具 Pro";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            LayoutBuilder.AddButtons(_ui, OnRefresh, OnMap, OnThreshold, OnCount, OnReset);

            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _strafeLogic = new StrafeLogic();
            _inputCore.Register(this.Handle);
            
            UpdateUiText();
        }

        private void ShowFeedbackBubble(string text, Color color)
        {
            // 不再需要列表维护，也不用管淡出动画了，直接清空
            while (_ui.PnlBubbleArea.Controls.Count > 0)
            {
                Control old = _ui.PnlBubbleArea.Controls[0];
                _ui.PnlBubbleArea.Controls.Remove(old);
                old.Dispose();
            }
            
            // 2. 创建新气泡
            var bubble = new BubbleControl(text, color);
            
            // 3. 设置位置 (居中, Y=10)
            // LeftPanel宽450, Bubble宽200 -> (450-200)/2 = 125
            bubble.Location = new Point(125, 10);
            
            // 4. 绑定自然销毁事件 (如果用户手速慢，气泡会自然淡出)
            bubble.AnimationComplete += (s, e) => {
                if (!_ui.PnlBubbleArea.IsDisposed && !bubble.IsDisposed)
                {
                    _ui.PnlBubbleArea.Controls.Remove(bubble);
                    bubble.Dispose();
                }
            };

            // 5. 添加到界面
            _ui.PnlBubbleArea.Controls.Add(bubble);
        }

        private void LogStrafeResult(StrafeResult r)
        {
            Color color = ColorHelper.GetColor(r.Latency);
            string evalRaw = ColorHelper.GetEvaluation(r.Latency);
            string evalText = (evalRaw == "Perfect" || evalRaw == "Great") ? evalRaw : Localization.Get("Eval_" + evalRaw);

            // 触发气泡
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
                
                // 清空气泡
                _ui.PnlBubbleArea.Controls.Clear();
                _activeBubbles.Clear();

                MessageBox.Show("已重置。");
            }
        }

        // ... (其他方法保持不变) ...

        private void UpdateUiText() {
            _ui.BtnRefresh.Text = Localization.Get("Btn_Refresh");
            _ui.BtnReset.Text = Localization.Get("Btn_Reset");
            _ui.BtnMap.Text = Localization.Get("Btn_Mapping");
            _ui.BtnThreshold.Text = Localization.Get("Btn_Threshold");
            _ui.BtnCount.Text = Localization.Get("Btn_Count");
            _ui.GraphAD.SetTitle(Localization.Get("Chart_AD_Title"));
            _ui.GraphWS.SetTitle(Localization.Get("Chart_WS_Title"));
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
        
        private void OnRefresh(object s, EventArgs e) { OnReset(s, e); }
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