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
            this.Text = "CS2 急停评估工具 Pro";
            this.BackColor = UiFactory.ColorBack;
            this.ForeColor = UiFactory.ColorText;

            _ui = LayoutBuilder.Build(this);
            
            // 绑定按钮事件：第一个按钮改为触发模拟测试
            LayoutBuilder.AddButtons(_ui, OnTestModeToggle, OnMap, OnThreshold, OnCount, OnReset);
            
            // 绑定模拟模式下的退出按钮
            if (_ui.BtnExitSim != null)
            {
                _ui.BtnExitSim.Click += (s, e) => ToggleSimulationMode(false);
            }

            _inputCore = new InputCore();
            _inputCore.OnGameKeyEvent += OnGameKey;
            _inputCore.OnFireEvent += OnFireInput; // 订阅鼠标开火事件

            _strafeLogic = new StrafeLogic();
            
            // 注册窗口句柄以监听 Raw Input
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
                _ui.LblSimStatus.Text = "准备就绪：请执行急停 + 开火";
                _ui.LblSimStatus.ForeColor = Color.White;
            }
            if (_ui.LblSimResult != null)
            {
                _ui.LblSimResult.Text = "等待操作...";
            }
        }

        #endregion

        #region 输入处理逻辑

        private void OnFireInput(long fireTick)
        {
            if (!_isSimMode || _simResultShown) return;

            // 记录开火的高精度时间戳
            _simLastFireTick = fireTick;

            // 如果急停数据已捕获，立即计算综合结果
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
                    string status = _isDebugMode ? "开启" : "关闭";
                    _ui.ListHistory.Items.Insert(0, $"[SYSTEM] >>> 调试模式已{status} <<<");
                    return;
                }
                // F10: [新增] 随机数据测试
                if (e.PhysicalKey == Keys.F10 && e.IsDown)
                {
                    SimulateTestData();
                    return;
                }

                // 2. 调试输出逻辑
                if (_isDebugMode)
                {
                    string action = e.IsDown ? "被按下" : "被松开";
                    _ui.ListHistory.Items.Insert(0, $"[DEBUG] {e.PhysicalKey} {action} (逻辑映射: {e.LogicKey})");
                }

                // 3. 更新 UI 指示灯
                UpdateKeyColor(e.LogicKey, e.IsDown);
            }));

            // 4. 急停核心判定 (F11 除外)
            if (e.LogicKey != Keys.F11 && e.LogicKey != Keys.F10)
            {
                var result = _strafeLogic.ProcessLogicKey(e.LogicKey, e.IsDown);
                
                if (result != null && result.IsValid) 
                {
                    if (!_isSimMode)
                    {
                        // 常规模式：记录并显示气泡
                        if (Math.Abs(result.Latency) <= _threshold)
                        {
                            this.Invoke(new Action(() => LogStrafeResult(result)));
                        }
                    }
                    else if (!_simResultShown)
                    {
                        // 模拟模式：暂存急停结果，等待开火
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

            // 1. 随机生成急停延迟 (-20ms 到 20ms 之间)
            // 负数代表 Overlap（重叠），正数代表 Gap（漂移）
            double mockStrafeLatency = (_rng.NextDouble() * 40.0) - 20.0;

            // 2. 随机生成开火延迟 (-15ms 到 40ms 之间)
            // 负数代表急停没结束就开火，正数代表结束后开火
            double mockShootDelay = (_rng.NextDouble() * 55.0) - 15.0;

            _simLastStrafe = new StrafeResult
            {
                Axis = _rng.Next(0, 2) == 0 ? "AD" : "WS",
                Latency = mockStrafeLatency,
                StopTick = now, // 以当前时刻为急停完成点
                ReleaseKey = Keys.A,
                PressKey = Keys.D
            };

            // 3. 计算模拟开火的 Tick
            _simLastFireTick = now + (long)(mockShootDelay * freq / 1000.0);
            _simResultShown = false;

            _ui.ListHistory.Items.Insert(0, $"[TEST] 已注入模拟数据: Strafe={mockStrafeLatency:F1}ms, FireDelay={mockShootDelay:F1}ms");
            
            CalculateSimResult();
        }

        #endregion

        #region 结果计算与显示

        private void CalculateSimResult()
{
    // 基础校验：确保数据完整且未重复判定
    if (_simLastStrafe == null || _simLastFireTick == 0 || _simResultShown) return;
    
    _simResultShown = true;

    // 1. 获取两项核心数据：急停物理延迟与射击延迟
    long freq = _strafeLogic.Frequency;
    double strafeLatency = _simLastStrafe.Latency; // 急停本身的物理延迟
    double shootDelayMs = (double)(_simLastFireTick - _simLastStrafe.StopTick) * 1000.0 / freq; // 急停完成到开火的延迟

    string grade = "FAIL";
    Color gradeColor = Color.Red;
    string detailTips = "";

    // 2. 综合判定逻辑
    
    // 情况 A：急停本身不合格 (判定绝对值 > 15ms)
    if (Math.Abs(strafeLatency) > 15.0)
    {
        grade = strafeLatency < 0 ? "急停过早" : "急停过迟";
        gradeColor = Color.Red;
        detailTips = "急停动作物理延迟超过 15ms，不合格";
    }
    // 情况 B：急停合格 (<= 15ms)，开始判定开火时机
    else if (shootDelayMs > -20 && shootDelayMs < 50)
    {
        grade = "PERFECT (完美)";
        gradeColor = Color.Gold;
        detailTips = "教科书级的急停射击！";
    }
    else if (shootDelayMs > -50 && shootDelayMs < 100)
    {
        grade = "GREAT (优秀)";
        gradeColor = Color.LimeGreen;
        detailTips = "非常出色的操作双手协同配合";
    }
    else if (shootDelayMs <= -50)
    {
        grade = "开火过早";
        gradeColor = Color.DeepSkyBlue;
        detailTips = "你在急停完成前过早击发";
    }
    else // shootDelayMs >= 100
    {
        grade = "开火过迟 (慢了)";
        gradeColor = Color.Tomato;
        detailTips = "急停完成后等待时间过长";
    }

    // 3. UI 更新显示
    if (_ui.LblSimStatus != null)
    {
        _ui.LblSimStatus.Text = grade;
        _ui.LblSimStatus.ForeColor = gradeColor;
    }

    if (_ui.LblSimResult != null)
    {
        _ui.LblSimResult.Text = $"[项1] 急停延迟: {strafeLatency:F1}ms\n" +
                                $"[项2] 射击延迟: {shootDelayMs:F1}ms\n\n" +
                                $"{detailTips}\n" +
                                $"2秒后系统重置...";
    }

    // 4. 自动重置定时器
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

        #region 基础窗体逻辑与事件处理

        protected override void WndProc(ref Message m) 
        {
            if (m.Msg == NativeMethods.WM_INPUT) _inputCore.ProcessMessage(m);
            base.WndProc(ref m);
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
            _ui.BtnTest.Text = "模拟测试"; 
            _ui.BtnReset.Text = Localization.Get("Btn_Reset");
            _ui.BtnMap.Text = Localization.Get("Btn_Mapping");
            _ui.BtnThreshold.Text = Localization.Get("Btn_Threshold");
            _ui.BtnCount.Text = Localization.Get("Btn_Count");
            _ui.GraphAD.SetTitle(Localization.Get("Chart_AD_Title"));
            _ui.GraphWS.SetTitle(Localization.Get("Chart_WS_Title"));
        }

        private void OnMap(object s, EventArgs e) 
        { 
             string val = InputBox.Show("映射配置", "请输入4个字母 (例如 WASD):");
             if (!string.IsNullOrEmpty(val)) _inputCore.UpdateMapping(val.ToUpper());
        }

        private void OnThreshold(object s, EventArgs e) 
        {
             string val = InputBox.Show("判定阈值", "输入毫秒数 (ms):", _threshold.ToString());
             if (int.TryParse(val, out int v)) _threshold = v;
        }

        private void OnCount(object s, EventArgs e) 
        {
             string val = InputBox.Show("样本上限", "显示最近次数:", _recCount.ToString());
             if (int.TryParse(val, out int v)) { _recCount = v; _ui.GraphAD.SetLimit(v); _ui.GraphWS.SetLimit(v); }
        }

        #endregion
    }
}