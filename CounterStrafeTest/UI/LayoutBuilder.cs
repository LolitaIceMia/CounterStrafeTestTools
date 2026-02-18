using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public class GameUiComponents
    {
        // 布局容器
        public TableLayoutPanel MainLayout;
        public Panel PnlGraph;
        public Panel PnlBubbleArea;
        public Panel PnlButtons;
        
        // 控件
        public AdvancedGraph GraphAD;
        public AdvancedGraph GraphWS;
        public ListBox ListHistory;
        
        // 按键指示灯
        public Label LblW, LblA, LblS, LblD;
        
        // 底部功能按钮
        public Button BtnTest;
        public Button BtnMap;
        public Button BtnThreshold;
        public Button BtnCount;
        public Button BtnReset;
        
        // 右上角功能按钮
        public Button BtnLang;
        public Button BtnMagnetDebug; // [新增] 磁轴调试按钮

        // 模拟测试模式覆盖层
        public Panel PnlSimulation;
        public Label LblSimStatus;
        public Label LblSimResult;
        public Button BtnExitSim;
    }

    public static class LayoutBuilder
    {
        // [修改] 增加 isMagnetic 参数
        public static GameUiComponents Build(MainForm form, bool isMagnetic)
        {
            form.Size = new Size(1000, 700);
            form.MinimumSize = new Size(800, 600);

            var comps = new GameUiComponents();

            // === 1. 主布局容器 ===
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 40, 10, 10) // Top Padding 40 为右上角按钮留出空间
            };
            
            // 列宽比例：左侧历史记录 30%，右侧图表 70%
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            
            // 行高比例：上部主要内容 85%，底部按钮栏 15%
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            form.Controls.Add(mainLayout);
            comps.MainLayout = mainLayout;

            // === 2. 左侧区域 (历史记录 + 按键灯) ===
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            
            // 按键指示灯区域 (WASD)
            var pnlKeys = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Height = 100, 
                ColumnCount = 3, 
                RowCount = 2 
            };
            for (int i = 0; i < 3; i++) pnlKeys.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pnlKeys.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlKeys.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            comps.LblW = UiFactory.CreateKeyLabel("W");
            comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S");
            comps.LblD = UiFactory.CreateKeyLabel("D");

            pnlKeys.Controls.Add(comps.LblW, 1, 0); // W 在第一行中间
            pnlKeys.Controls.Add(comps.LblA, 0, 1); // A 在第二行左边
            pnlKeys.Controls.Add(comps.LblS, 1, 1); // S 在第二行中间
            pnlKeys.Controls.Add(comps.LblD, 2, 1); // D 在第二行右边

            // 历史记录列表
            comps.ListHistory = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.None,
                IntegralHeight = false
            };

            // 气泡反馈区 (覆盖在列表上方或独立)
            comps.PnlBubbleArea = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };

            leftPanel.Controls.Add(comps.ListHistory); // 填充剩余空间
            leftPanel.Controls.Add(comps.PnlBubbleArea); // 顶部气泡
            leftPanel.Controls.Add(pnlKeys); // 最顶部按键
            
            mainLayout.Controls.Add(leftPanel, 0, 0);

            // === 3. 右侧区域 (图表) ===
            comps.PnlGraph = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 25) };
            var splitGraph = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 2, 
                ColumnCount = 1 
            };
            splitGraph.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            splitGraph.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            comps.GraphAD = new AdvancedGraph { Dock = DockStyle.Fill };
            comps.GraphWS = new AdvancedGraph { Dock = DockStyle.Fill };
            
            splitGraph.Controls.Add(comps.GraphAD, 0, 0);
            splitGraph.Controls.Add(comps.GraphWS, 0, 1);
            comps.PnlGraph.Controls.Add(splitGraph);

            mainLayout.Controls.Add(comps.PnlGraph, 1, 0);

            // === 4. 底部按钮区域 ===
            comps.PnlButtons = new Panel { Dock = DockStyle.Fill };
            var flowBtn = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };
            
            comps.BtnTest = UiFactory.CreateButton("Btn_Test", null);
            comps.BtnMap = UiFactory.CreateButton("Btn_Mapping", null);
            comps.BtnThreshold = UiFactory.CreateButton("Btn_Threshold", null);
            comps.BtnCount = UiFactory.CreateButton("Btn_Count", null);
            comps.BtnReset = UiFactory.CreateButton("Btn_Reset", UiFactory.ColorFail);

            // 注意：按钮事件绑定在 MainForm 中进行
            // 此处仅添加到布局
            // (MainForm 会调用 AddButtons 方法将它们加入 flowBtn)
            
            comps.PnlButtons.Controls.Add(flowBtn);
            mainLayout.Controls.Add(comps.PnlButtons, 0, 1);
            mainLayout.SetColumnSpan(comps.PnlButtons, 2); // 跨两列

            // === 5. 右上角：语言切换按钮 ===
            comps.BtnLang = new Button
            {
                Size = new Size(36, 36),
                Location = new Point(form.ClientSize.Width - 50, 5), // 右上角绝对定位
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                BackgroundImageLayout = ImageLayout.Zoom
            };
            comps.BtnLang.FlatAppearance.BorderSize = 0;
            comps.BtnLang.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            
            try 
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Language.ico");
                if (File.Exists(iconPath))
                {
                    using (Icon ico = new Icon(iconPath, 32, 32))
                    {
                        comps.BtnLang.Image = ico.ToBitmap();
                    }
                }
                else 
                {
                    comps.BtnLang.Text = "文";
                    comps.BtnLang.ForeColor = Color.White;
                }
            }
            catch { comps.BtnLang.Text = "CN"; comps.BtnLang.ForeColor = Color.White; }

            form.Controls.Add(comps.BtnLang);
            comps.BtnLang.BringToFront();

            // === 6. [新增] 右上角：磁轴调试按钮 (动态创建) ===
            if (isMagnetic)
            {
                comps.BtnMagnetDebug = new Button
                {
                    Size = new Size(100, 32),
                    // 放在语言按钮左侧，间距 10px (50 + 100 + 10 = 160)
                    Location = new Point(form.ClientSize.Width - 160, 7), 
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.Gold,
                    Cursor = Cursors.Hand,
                    Text = "磁轴调试", // 默认值，会被 UpdateUiText 覆盖
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                comps.BtnMagnetDebug.FlatAppearance.BorderSize = 1;
                comps.BtnMagnetDebug.FlatAppearance.BorderColor = Color.Gold;

                form.Controls.Add(comps.BtnMagnetDebug);
                comps.BtnMagnetDebug.BringToFront();
            }

            // === 7. 模拟测试全屏覆盖层 (默认隐藏) ===
            comps.PnlSimulation = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(20, 20, 20), 
                Visible = false 
            };
            
            var simCenter = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 3, 
                ColumnCount = 1 
            };
            simCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            simCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            simCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            comps.LblSimStatus = new Label 
            { 
                Text = "READY", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 36, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Fill 
            };
            
            comps.LblSimResult = new Label 
            { 
                Text = "", 
                ForeColor = Color.LightGray, 
                Font = new Font("Segoe UI", 16), 
                TextAlign = ContentAlignment.TopCenter, 
                Dock = DockStyle.Fill 
            };
            
            comps.BtnExitSim = UiFactory.CreateButton("Btn_Exit_Sim", null);
            comps.BtnExitSim.Anchor = AnchorStyles.None;
            comps.BtnExitSim.Size = new Size(200, 50);

            simCenter.Controls.Add(comps.LblSimStatus, 0, 0);
            simCenter.Controls.Add(comps.LblSimResult, 0, 1);
            simCenter.Controls.Add(comps.BtnExitSim, 0, 2);

            comps.PnlSimulation.Controls.Add(simCenter);
            form.Controls.Add(comps.PnlSimulation);
            comps.PnlSimulation.BringToFront();

            return comps;
        }

        // 辅助方法：将按钮添加到流式布局中
        public static void AddButtons(GameUiComponents ui, EventHandler onTest, EventHandler onMap, EventHandler onThres, EventHandler onCount, EventHandler onReset)
        {
            var flow = ui.PnlButtons.Controls[0] as FlowLayoutPanel;
            if (flow == null) return;

            ui.BtnTest.Click += onTest;
            ui.BtnMap.Click += onMap;
            ui.BtnThreshold.Click += onThres;
            ui.BtnCount.Click += onCount;
            ui.BtnReset.Click += onReset;

            flow.Controls.Add(ui.BtnTest);
            flow.Controls.Add(ui.BtnMap);
            flow.Controls.Add(ui.BtnThreshold);
            flow.Controls.Add(ui.BtnCount);
            flow.Controls.Add(ui.BtnReset);
        }
    }
}