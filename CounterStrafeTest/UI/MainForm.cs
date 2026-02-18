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
        
        private Random _rng = new Random();
        private int _threshold = 120;
        private int _recCount = 40;

        // --- 模拟测试模式变量 ---
        private bool _isSimMode = false;
        private StrafeResult _simLastStrafe = null;
        private long _simLastFireTick = 0;
        private bool _simResultShown = false;

        // --- 调试模式变量 ---
        private bool _isDebugMode = false;

        public MainForm()
        {
            // 基础窗体设置
            this.Text = "CS2 急停评估工具 Pro";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            
            // 1. 绑定语言切换按钮 (右上角)
            if (_ui.BtnLang != null)
            {
                _ui.BtnLang.Click += (s, e) => 
                {
                    Localization.CurrentLanguage = (Localization.CurrentLanguage == AppLanguage.Chinese) 
                        ? AppLanguage.English : AppLanguage.Chinese;
                    UpdateUiText();
                };
            }

            // 2. 绑定主界面功能按钮
            LayoutBuilder.AddButtons(_ui, OnTestModeToggle, OnMap, OnThreshold, OnCount, OnReset);
            
            // 3. 绑定模拟测试界面专用退出按钮
            if (_ui.BtnExitSim != null)
            {
                _ui.BtnExitSim.Click += (s, e) => ToggleSimulationMode(false);
            }

            // 4. 初始化核心逻辑
            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _inputCore.OnFireEvent += OnFireInput; 

            _strafeLogic = new StrafeLogic();
            
            // 5. 注册 Raw Input 监听
            _inputCore.Register(this.Handle);
            
            UpdateUiText();
        }

        #region 模式切换逻辑

        private void OnTestModeToggle(object s, EventArgs e)
        {
            ToggleSimulationMode(true);
        }

        private void ToggleSimulationMode(bool enable)
        {
            _isSimMode = enable;
            if (_ui.PnlSimulation != null)
            {
                _ui.PnlSimulation.Visible = enable;
                _ui.PnlSimulation.BringToFront();
            }
            
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
            
            if (_ui.LblSimStatus != null)
            {
                _ui.LblSimStatus.Text = Localization.Get("Sim_Ready");
                _ui.LblSimStatus.ForeColor = Color.White;
            }
            if (_ui.LblSimResult != null)
            {
                _ui.LblSimResult.Text = Localization.Get("Sim_Waiting");
            }
        }

        #endregion

        #region 输入处理逻辑

        private void OnFireInput(long fireTick)
        {
            if (!_isSimMode || _simResultShown) return;

            // 记录开火时刻 (QPC Ticks)
            _simLastFireTick = fireTick;

            // 如果急停数据已捕获，立即进行判定
            if (_simLastStrafe != null)
            {
                this.Invoke(new Action(CalculateSimResult));
            }
        }

        private void OnGameKey(object sender, GameKeyEventArgs e) 
        {
            this.Invoke(new Action(() => 
            {
                // 1. 处理调试模式开关 (F11 按下)
                if (e.PhysicalKey == Keys.F11 && e.IsDown)
                {
                    _isDebugMode = !_isDebugMode;
                    string status = _isDebugMode ? "ON" : "OFF";
                    _ui.ListHistory.Items.Insert(0, $"[SYSTEM] Debug Mode: {status}");
                    return;
                }

                // 2. F10 随机数据模拟输入
                if (e.PhysicalKey == Keys.F10 && e.IsDown)
                {
                    SimulateTestData();
                    return;
                }

                // 3. 调试输出按键流
                if (_isDebugMode)
                {
                    string action = e.IsDown ? Localization.Get("Status_Pressed") : Localization.Get("Status_Released");
                    _ui.ListHistory.Items.Insert(0, $"[DEBUG] {e.PhysicalKey} {action} (Logic: {e.LogicKey})");
                }

                // 4. 更新 UI 按键指示灯
                UpdateKeyColor(e.LogicKey, e.IsDown);
            }));

            // 5. 核心急停逻辑判定 (排除系统功能键)
            if (e.LogicKey != Keys.F11 && e.LogicKey != Keys.F10)
            {
                var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
                
                if (result != null && result.IsValid) 
                {
                    if (!_isSimMode)
                    {
                        // 常规练习模式：过滤后显示结果
                        if (Math.Abs(result.Latency) <= _threshold)
                        {
                            this.Invoke(new Action(() => LogStrafeResult(result)));
                        }
                    }
                    else if (!_simResultShown)
                    {
                        // 模拟测试模式：暂存急停数据，等待射击动作
                        _simLastStrafe = result;
                        if (_simLastFireTick > 0)
                        {
                            this.Invoke(new Action(CalculateSimResult));
                        }
                    }
                }
            }
        }

        private void SimulateTestData()
        {
            if (!_isSimMode) ToggleSimulationMode(true);

            long freq = _strafeLogic.Frequency;
            NativeMethods.QueryPerformanceCounter(out long now);

            // 随机生成测试数据
            double mockStrafeLatency = (_rng.NextDouble() * 40.0) - 20.0;
            double mockShootDelay = (_rng.NextDouble() * 200.0) - 70.0;

            _simLastStrafe = new StrafeResult
            {
                Axis = _rng.Next(0, 2) == 0 ? "AD" : "WS",
                Latency = mockStrafeLatency,
                StopTick = now,
                ReleaseKey = Keys.A,
                PressKey = Keys.D
            };

            _simLastFireTick = now + (long)(mockShootDelay * freq / 1000.0);
            _simResultShown = false;

            _ui.ListHistory.Items.Insert(0, $"[TEST] Mock Input: Strafe={mockStrafeLatency:F1}ms, FireDelay={mockShootDelay:F1}ms");
            
            CalculateSimResult();
        }

        #endregion

        #region 结果计算与显示

        private void CalculateSimResult()
        {
            if (_simLastStrafe == null || _simLastFireTick == 0 || _simResultShown) return;
            _simResultShown = true;

            long freq = _strafeLogic.Frequency;
            double strafeLatency = _simLastStrafe.Latency; 
            double shootDelayMs = (double)(_simLastFireTick - _simLastStrafe.StopTick) * 1000.0 / freq; 

            string gradeKey, tipKey;
            Color gradeColor;

            // 1. 综合判定逻辑
            if (Math.Abs(strafeLatency) > 15.0)
            {
                gradeKey = strafeLatency < 0 ? "Grade_Early_Strafe_Overlap" : "Grade_Late_Strafe_Slow";
                gradeColor = Color.Red;
                tipKey = "Tip_Strafe_Fail";
            }
            else if (shootDelayMs > -20 && shootDelayMs < 50)
            {
                gradeKey = "Grade_Perfect";
                gradeColor = Color.Gold;
                tipKey = "Tip_Perfect";
            }
            else if (shootDelayMs > -50 && shootDelayMs < 100)
            {
                gradeKey = "Grade_Great";
                gradeColor = Color.LimeGreen;
                tipKey = "Tip_Great";
            }
            else if (shootDelayMs <= -50)
            {
                gradeKey = "Grade_Early_Fire";
                gradeColor = Color.DeepSkyBlue;
                tipKey = "Tip_Fire_Early";
            }
            else 
            {
                gradeKey = "Grade_Late_Fire";
                gradeColor = Color.Tomato;
                tipKey = "Tip_Fire_Late";
            }

            // 2. 更新模拟界面文本
            if (_ui.LblSimStatus != null)
            {
                _ui.LblSimStatus.Text = Localization.Get(gradeKey);
                _ui.LblSimStatus.ForeColor = gradeColor;
            }

            if (_ui.LblSimResult != null)
            {
                _ui.LblSimResult.Text = $"{Localization.Get("Item_1")}: {strafeLatency:F1}ms\n" +
                                        $"{Localization.Get("Item_2")}: {shootDelayMs:F1}ms\n\n" +
                                        $"{Localization.Get(tipKey)}\n" +
                                        $"{Localization.Get("Sim_Reset_Tip")}";
            }

            // 3. 自动重置定时器 (显式指定命名空间解决歧义)
            System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            resetTimer.Tick += (s, args) => 
            {
                resetTimer.Stop();
                resetTimer.Dispose();
                if (_isSimMode) ResetSimState();
            };
            resetTimer.Start();
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

        private void ShowFeedbackBubble(string text, Color color)
        {
            if (_ui.PnlBubbleArea.IsDisposed) return;
            while (_ui.PnlBubbleArea.Controls.Count > 0)
            {
                Control old = _ui.PnlBubbleArea.Controls[0];
                _ui.PnlBubbleArea.Controls.Remove(old);
                old.Dispose();
            }
            var bubble = new BubbleControl(text, color) { Location = new Point(20, 20) };
            bubble.AnimationComplete += (s, e) => 
            {
                if (!_ui.PnlBubbleArea.IsDisposed && !bubble.IsDisposed)
                {
                    _ui.PnlBubbleArea.Controls.Remove(bubble);
                    bubble.Dispose();
                }
            };
            _ui.PnlBubbleArea.Controls.Add(bubble);
        }

        private void UpdateKeyColor(Keys k, bool isDown) 
        {
            Label t = k switch { 
                Keys.W => _ui.LblW, Keys.A => _ui.LblA, 
                Keys.S => _ui.LblS, Keys.D => _ui.LblD, _ => null 
            };
            if (t != null) t.BackColor = isDown ? Color.LimeGreen : Color.LightGray;
        }

        #endregion

        #region 基础窗体逻辑与本地化处理

        protected override void WndProc(ref Message m) 
        {
            if (m.Msg == NativeMethods.WM_INPUT) _inputCore.ProcessMessage(m);
            base.WndProc(ref m);
        }

        private void OnReset(object s, EventArgs e)
        {
            if (MessageBox.Show(Localization.Get("Dialog_Reset_Confirm"), Localization.Get("Dialog_Reset_Title"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _threshold = 120;
                _recCount = 40;
                _inputCore.ResetMapping();
                _ui.ListHistory.Items.Clear();
                _ui.GraphAD.Clear(); _ui.GraphAD.SetLimit(_recCount);
                _ui.GraphWS.Clear(); _ui.GraphWS.SetLimit(_recCount);
                _strafeLogic.Reset();
                _ui.PnlBubbleArea.Controls.Clear();
            }
        }

        private void UpdateUiText() 
        {
            this.Text = Localization.Get("Title");
            _ui.BtnTest.Text = Localization.Get("Btn_Test");
            _ui.BtnReset.Text = Localization.Get("Btn_Reset");
            _ui.BtnMap.Text = Localization.Get("Btn_Mapping");
            _ui.BtnThreshold.Text = Localization.Get("Btn_Threshold");
            _ui.BtnCount.Text = Localization.Get("Btn_Count");
            _ui.BtnExitSim.Text = Localization.Get("Btn_Exit_Sim");

            _ui.GraphAD.SetTitle(Localization.Get("Chart_AD_Title"));
            _ui.GraphWS.SetTitle(Localization.Get("Chart_WS_Title"));
            
            if (_isSimMode && !_simResultShown) ResetSimState();
        }

        private void OnMap(object s, EventArgs e) 
        { 
             string val = InputBox.Show(Localization.Get("Dialog_Map_Title"), Localization.Get("Dialog_Map_Msg"));
             if (!string.IsNullOrEmpty(val)) _inputCore.UpdateMapping(val.ToUpper());
        }

        private void OnThreshold(object s, EventArgs e) 
        {
             string val = InputBox.Show(Localization.Get("Dialog_Thres_Title"), Localization.Get("Dialog_Thres_Msg"), _threshold.ToString());
             if (int.TryParse(val, out int v)) _threshold = v;
        }

        private void OnCount(object s, EventArgs e) 
        {
             string val = InputBox.Show(Localization.Get("Dialog_Count_Title"), Localization.Get("Dialog_Count_Msg"), _recCount.ToString());
             if (int.TryParse(val, out int v)) { _recCount = v; _ui.GraphAD.SetLimit(v); _ui.GraphWS.SetLimit(v); }
        }

        #endregion
    }
}