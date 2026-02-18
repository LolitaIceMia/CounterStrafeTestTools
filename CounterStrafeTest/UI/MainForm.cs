using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CounterStrafeTest.Core; // 引用核心层

namespace CounterStrafeTest.UI
{
    public partial class MainForm : Form
    {
        // ==================== 1. 控件声明 ====================
        private Label lblFeedback;
        private Label lblKeyStatusA;
        private Label lblKeyStatusD;
        private ListBox listHistory;
        private SimpleGraph graphAD;    // 我们自定义的波形图控件
        private ComboBox comboLanguage; // 语言选择框
        private Label lblLangTitle;     // 语言标题
        private ProgressBar pbMagneticSim; // 磁轴模拟进度条 (仅磁轴模式显示)

        // ==================== 2. 逻辑变量 ====================
        private long _freq; // CPU 计时频率
        // 记录按键状态 (True=按下, False=松开)
        private Dictionary<Keys, bool> _keyState = new Dictionary<Keys, bool>();
        
        // 急停逻辑相关
        private bool _waitingForCounterStrafe = false; // 是否正在等待急停的第二段操作
        private Keys _releaseKey; // 刚刚松开的键 (例如 A)
        private long _releaseTime; // 松开的时间点

        public MainForm()
        {
            // 1. 初始化界面布局
            InitializeCustomUI();
            
            // 2. 初始化底层逻辑 (API注册、计时器)
            InitializeLogic(); 
            
            // 3. 根据默认语言刷新一次界面文字
            UpdateUIText(); 
        }

        // ==================== 3. 初始化逻辑 ====================
        private void InitializeLogic()
        {
            // 获取高精度计时器的频率
            NativeMethods.QueryPerformanceFrequency(out _freq);
            
            // 初始化按键字典，防止 KeyNotFound 报错
            _keyState[Keys.A] = false;
            _keyState[Keys.D] = false;

            // 注册 Windows Raw Input (原始输入)
            RegisterRawInput();
        }

        private void RegisterRawInput()
        {
            var rid = new NativeMethods.RAWINPUTDEVICE[1];
            rid[0].usUsagePage = 0x01; // 通用桌面设备
            rid[0].usUsage = 0x06;     // 键盘
            rid[0].dwFlags = NativeMethods.RIDEV_INPUTSINK; // 后台也能接收
            rid[0].hwndTarget = this.Handle;

            if (!NativeMethods.RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
            {
                MessageBox.Show("无法注册原始输入设备，程序可能无法正常工作。\n请尝试以管理员身份运行。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== 4. 核心消息循环 (WndProc) ====================
        protected override void WndProc(ref Message m)
        {
            // 拦截 WM_INPUT 消息
            if (m.Msg == NativeMethods.WM_INPUT)
            {
                ProcessRawInput(m.LParam);
            }
            base.WndProc(ref m);
        }

        private void ProcessRawInput(IntPtr lParam)
        {
            uint dwSize = 0;
            // 第一步：获取数据大小
            NativeMethods.GetRawInputData(lParam, NativeMethods.RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                // 第二步：获取实际数据
                if (NativeMethods.GetRawInputData(lParam, NativeMethods.RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER))) == dwSize)
                {
                    var raw = Marshal.PtrToStructure<NativeMethods.RAWINPUT>(buffer);
                    if (raw.header.dwType == NativeMethods.RIM_TYPEKEYBOARD)
                    {
                        ushort vKey = raw.keyboard.VKey;
                        // Flags: 0=按下, 1=松开
                        bool isDown = (raw.keyboard.Flags & 1) == 0;

                        if (vKey == 65) HandleKey(Keys.A, isDown); // A键
                        if (vKey == 68) HandleKey(Keys.D, isDown); // D键
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer); // 别忘了释放非托管内存
            }
        }

        // ==================== 5. 业务逻辑处理 ====================
        private void HandleKey(Keys key, bool isDown)
        {
            // 状态去重：如果状态没变，就不处理 (Raw Input 有时会重复发送按下信号)
            if (_keyState.ContainsKey(key) && _keyState[key] == isDown) return;

            _keyState[key] = isDown;
            UpdateKeyUI(key, isDown); // 刷新界面上的按键颜色

            // 获取当前高精度时间
            NativeMethods.QueryPerformanceCounter(out long now);

            if (isDown)
            {
                // [按下逻辑]
                // 如果正在等待急停 (即对面那个键刚松开，且在等待反向键按下)
                if (_waitingForCounterStrafe)
                {
                    // 判断是否是反向键 (A松->D按，或 D松->A按)
                    bool isCounter = (_releaseKey == Keys.A && key == Keys.D) || (_releaseKey == Keys.D && key == Keys.A);
                    
                    if (isCounter)
                    {
                        // 计算延迟 (毫秒)
                        double latencyMs = (double)(now - _releaseTime) * 1000.0 / _freq;
                        
                        // 记录结果
                        RecordResult(latencyMs, _releaseKey, key);
                        
                        // 急停完成，重置状态
                        _waitingForCounterStrafe = false;
                    }
                }
            }
            else
            {
                // [松开逻辑]
                // 记录松开哪个键、松开的时间，并开始等待
                _releaseKey = key;
                _releaseTime = now;
                _waitingForCounterStrafe = true;
                
                // (可选优化：在这里可以启动一个 Timer，如果超过 500ms 还没按反向键，就取消等待，防止误判)
            }
        }

        private void RecordResult(double ms, Keys release, Keys press)
        {
            // 1. 根据延迟判断评价 (从多语言类获取文本)
            string evalKey = "Eval_Perfect"; // 默认 key
            Color color = Color.LimeGreen;

            if (ms < 0) { evalKey = "Eval_Early"; color = Color.Cyan; }
            else if (ms > 50) { evalKey = "Eval_Late"; color = Color.OrangeRed; }

            string evalText = Localization.Get(evalKey); // 获取翻译

            // 2. 格式化日志
            string log = $"[{DateTime.Now:HH:mm:ss.fff}] {release}->{press}: {ms:F2}ms ({evalText})";
            
            // 3. 插入列表 (最新的在最上面)
            listHistory.Items.Insert(0, log);
            // 保持列表不超过 100 条
            if(listHistory.Items.Count > 100) listHistory.Items.RemoveAt(100);

            // 4. 更新顶部大字反馈
            lblFeedback.Text = $"{evalText}: {ms:F2}ms";
            lblFeedback.BackColor = color;

            // 5. 更新图表
            graphAD.AddPoint((float)ms);
        }

        // ==================== 6. 界面更新与多语言 ====================
        
        // 刷新按键状态 Label 的文字和颜色
        private void UpdateKeyUI(Keys key, bool isDown)
        {
            Label target = key == Keys.A ? lblKeyStatusA : lblKeyStatusD;
            string keyName = key == Keys.A ? Localization.Get("Key_A") : Localization.Get("Key_D");
            string status = isDown ? Localization.Get("Status_Pressed") : Localization.Get("Status_Released");
            
            // 因为这是在非 UI 线程(RawInput)触发的，所以要用 Invoke 确保线程安全
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateKeyUI(key, isDown)));
                return;
            }

            target.Text = $"{keyName}: {status}";
            target.BackColor = isDown ? Color.LightGreen : Color.LightGray;
        }

        // 统一刷新所有静态文本 (切换语言时调用)
        private void UpdateUIText()
        {
            this.Text = Localization.Get("Title");
            lblFeedback.Text = Localization.Get("Feedback_Default");
            graphAD.SetTitle(Localization.Get("Chart_Title"));
            lblLangTitle.Text = Localization.Get("Lang_Switch");

            // 刷新按键上的文字
            UpdateKeyUI(Keys.A, _keyState[Keys.A]);
            UpdateKeyUI(Keys.D, _keyState[Keys.D]);
        }

        // ==================== 7. 手写界面布局代码 ====================
        private void InitializeCustomUI()
        {
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.DoubleBuffered = true;

            // --- 顶部面板 (反馈 + 语言切换) ---
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(50, 50, 50) };
            
            // 语言切换区 (右上角)
            Panel langPanel = new Panel { Dock = DockStyle.Right, Width = 200, BackColor = Color.Transparent };
            lblLangTitle = new Label { Text = "Language", Top = 10, Left = 10, ForeColor = Color.White, AutoSize = true };
            comboLanguage = new ComboBox { Top = 35, Left = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            comboLanguage.Items.Add("中文 (Chinese)");
            comboLanguage.Items.Add("English");
            comboLanguage.SelectedIndex = (int)Localization.CurrentLanguage;
            
            // 绑定事件：选择改变时刷新界面
            comboLanguage.SelectedIndexChanged += (s, e) => {
                Localization.CurrentLanguage = (AppLanguage)comboLanguage.SelectedIndex;
                UpdateUIText(); 
            };

            langPanel.Controls.Add(lblLangTitle);
            langPanel.Controls.Add(comboLanguage);

            // 大字反馈 Label
            lblFeedback = new Label
            {
                Text = "...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
            };

            topPanel.Controls.Add(lblFeedback); 
            topPanel.Controls.Add(langPanel);   
            this.Controls.Add(topPanel);

            // --- 左侧面板 (按键状态 + 历史列表) ---
            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 400, Padding = new Padding(10) };
            
            lblKeyStatusA = CreateStatusLabel();
            lblKeyStatusD = CreateStatusLabel();
            lblKeyStatusD.Top = 50; // D键在下面

            listHistory = new ListBox
            {
                Top = 100, Width = 380, Height = 400,
                BackColor = Color.FromArgb(40,40,40),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            leftPanel.Controls.Add(lblKeyStatusA);
            leftPanel.Controls.Add(lblKeyStatusD);
            leftPanel.Controls.Add(listHistory);
            this.Controls.Add(leftPanel);

            // --- 右侧面板 (图表 + 磁轴进度条) ---
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 如果是磁轴模式，显示底部的进度条 (Program.IsMagneticMode 是之前在 Program.cs 定义的)
            if (Program.IsMagneticMode)
            {
                pbMagneticSim = new ProgressBar { Dock = DockStyle.Bottom, Height = 30, Maximum = 100 };
                rightPanel.Controls.Add(pbMagneticSim);
            }

            graphAD = new SimpleGraph { Dock = DockStyle.Fill };
            rightPanel.Controls.Add(graphAD);
            this.Controls.Add(rightPanel);
        }

        // 辅助方法：快速创建统一风格的 Label
        private Label CreateStatusLabel()
        {
            return new Label
            {
                AutoSize = false, Size = new Size(380, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Microsoft YaHei", 12, FontStyle.Bold)
            };
        }
    }
}