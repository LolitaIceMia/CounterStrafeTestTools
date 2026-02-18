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
        // --- 模拟模式变量 ---
        private bool _isSimMode = false;
        private StrafeResult _simLastStrafe = null;
        private long _simLastFireTick = 0;
        private bool _simResultShown = false;
        // -------------------

        public MainForm()
        {
            this.Text = "CS2 急停评估工具 Pro";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            
            // 绑定按钮事件，注意这里第一个参数是 OnTest
            LayoutBuilder.AddButtons(_ui, OnTest, OnMap, OnThreshold, OnCount, OnReset);
            _ui.BtnExitSim.Click += (s, e) => ToggleSimulationMode(false);

            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _inputCore.OnFireEvent += OnFireInput; // 绑定开火事件
            _strafeLogic = new StrafeLogic();
            _inputCore.Register(this.Handle);
            
            UpdateUiText();
        }
        
        // 切换常规模式/模拟模式
        private void OnTestModeToggle(object s, EventArgs e)
        {
            ToggleSimulationMode(true);
        }

        private void ToggleSimulationMode(bool enable)
        {
            _isSimMode = enable;
            _ui.PnlSimulation.Visible = enable;
            
            if (enable)
            {
                ResetSimState();
            }
        }
        private void ResetSimState()
        {
            _simLastStrafe = null;
            _simLastFireTick = 0;
            _simResultShown = false;
            _ui.LblSimStatus.Text = "请急停 + 开火 (Strafe + Fire)";
            _ui.LblSimStatus.ForeColor = Color.White;
            _ui.LblSimResult.Text = "等待操作...";
        }

        // 处理开火逻辑 (Mouse Click)
        private void OnFireInput(long fireTick)
        {
            if (!_isSimMode || _simResultShown) return;

            // 记录开火时间
            _simLastFireTick = fireTick;

            // 如果已经检测到急停，立即计算结果
            if (_simLastStrafe != null)
            {
                CalculateSimResult();
            }
            // 如果还没检测到急停，可能用户先开火了（Too Early），
            // 或者急停逻辑还没跑完，我们给一点点缓冲，或者直接等待急停事件触发计算
        }

        // 处理键盘逻辑
        private void OnGameKey(object sender, GameKeyEventArgs e) 
        {
            this.Invoke(new Action(() => UpdateKeyColor(e.LogicKey, e.IsDown)));
            var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
             
            if (result != null && result.IsValid) 
            {
                // 常规模式逻辑
                if (!_isSimMode && Math.Abs(result.Latency) <= _threshold) {
                    this.Invoke(new Action(() => LogStrafeResult(result)));
                }
                // 模拟模式逻辑
                else if (_isSimMode && !_simResultShown)
                {
                    _simLastStrafe = result;
                    this.Invoke(new Action(() => 
                    {
                        // 如果已经开过火了，立即计算
                        if (_simLastFireTick > 0) CalculateSimResult();
                        // 否则等待开火
                    }));
                }
            }
        }
        
        private void CalculateSimResult()
        {
            if (_simLastStrafe == null || _simLastFireTick == 0) return;
            
            _simResultShown = true; // 锁定结果，防止多次触发

            // 1. 计算射击延迟 (Shooting Latency)
            // TimeDiff = FireTime - StrafeStopTime
            // 正数 = 急停后开火 (正常)
            // 负数 = 急停完成前开火 (过早)
            long freq = _strafeLogic.Frequency;
            double shootDelayMs = (double)(_simLastFireTick - _simLastStrafe.StopTick) * 1000.0 / freq;

            // 2. 评级
            string grade = "MISS";
            Color gradeColor = Color.Gray;

            if (shootDelayMs < 0)
            {
                grade = "TOO EARLY (过早)";
                gradeColor = Color.Red;
            }
            else if (shootDelayMs <= 5.0)
            {
                grade = "PERFECT (完美)";
                gradeColor = Color.Gold; // 金色传说
            }
            else if (shootDelayMs <= 10.0)
            {
                grade = "GREAT (优秀)";
                gradeColor = Color.LimeGreen;
            }
            else
            {
                grade = "LATE (过迟)";
                gradeColor = Color.Orange;
            }

            // 3. 构造显示文本
            string strafeInfo = $"急停延迟: {_simLastStrafe.Latency:F1}ms ({_simLastStrafe.Axis})";
            string shootInfo = $"射击延迟: {shootDelayMs:F1}ms";
            
            _ui.LblSimStatus.Text = grade;
            _ui.LblSimStatus.ForeColor = gradeColor;
            _ui.LblSimResult.Text = $"{strafeInfo}\n{shootInfo}\n\n再次操作以重试...";

            // 使用显式命名空间引用，消除不明确的引用错误
            System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            resetTimer.Tick += (s, args) => {
                // 停止并释放计时器，防止内存泄漏和重复触发
                resetTimer.Stop();
                resetTimer.Dispose();

                // 只有在依然处于模拟模式时才重置状态
                if (_isSimMode) 
                {
                    ResetSimState();
                }
            };
            resetTimer.Start();
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

        private void OnTest(object s, EventArgs e)
        {
            // 切换到模拟测试模式（即您要求的 UI 刷新为模拟页面）
            ToggleSimulationMode(true); 
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