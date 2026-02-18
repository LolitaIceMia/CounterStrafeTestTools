using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    // 定义 UI 组件容器，方便 MainForm 访问
    public class GameUiComponents
    {
        public Panel PnlBubbleArea;     // 气泡专用区域
        public FlowLayoutPanel PnlButtons; // 底部按钮区域
        public Label LblW, LblA, LblS, LblD; // WASD 状态灯
        public ListBox ListHistory;     // 历史日志列表
        public AdvancedGraph GraphAD, GraphWS; // 两个图表
        public Button BtnTest;          // 测试按钮 (原刷新)
        public Button BtnMap, BtnThreshold, BtnCount, BtnReset; // 其他功能按钮
        public Panel LeftPanel;         // 左侧主面板
        public Panel PnlSimulation; // 模拟模式的覆盖层
        public Label LblSimStatus;  // 模拟模式状态 (READY / BAM!)
        public Label LblSimResult;  // 模拟模式结果详情
        public Button BtnExitSim;   // 退出模拟
        public Button BtnLang; //语言切换
    }

    public static class LayoutBuilder
    {
        public static GameUiComponents Build(Form form)
        {
            // 1. 设置窗体基础属性
            form.ClientSize = new Size(1280, 720);
            form.StartPosition = FormStartPosition.CenterScreen;

            var comps = new GameUiComponents();

            // 2. 主容器 (左右分栏)
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F)); // 左侧固定宽 450
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // 右侧自适应
            form.Controls.Add(mainLayout);
            comps.BtnLang = new Button
            {
                Size = new Size(10, 10),
                // 距离右侧 45 像素（考虑滚动条空间），上方 10 像素
                Location = new Point(form.ClientSize.Width - 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TextImageRelation = TextImageRelation.ImageAboveText
            };
            try 
            {
                // 假设图标位于程序运行目录下的 Resources 文件夹中
                comps.BtnLang.Image = Image.FromFile("Resources/Language.ico");
            }
            catch 
            {
                // 如果加载失败，显示文字占位
                comps.BtnLang.Text = "L";
                comps.BtnLang.ForeColor = Color.White;
            }
            
            form.Controls.Add(comps.BtnLang);
            comps.BtnLang.BringToFront();
            comps.BtnLang.FlatAppearance.BorderSize = 0;

            // === 左侧面板构建 ===
            comps.LeftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            // (1) 气泡区域 (Dock=Top)
            // 高度 = 20(TopMargin) + 50(BubbleHeight) + 20(BottomMargin) = 90
            comps.PnlBubbleArea = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 90, 
                BackColor = Color.Transparent 
            };

            // (2) 按键网格 (Dock=Top)
            // 位于气泡区域下方
            var keyContainer = new Panel { Dock = DockStyle.Top, Height = 180 }; 
            var keysGrid = new TableLayoutPanel 
            { 
                Width = 260, Height = 170, 
                ColumnCount = 3, RowCount = 2,
                // 居中计算: (450 - 260) / 2 = 95
                Left = 95, 
                Top = 0 
            };
            // 创建 WASD 标签
            comps.LblW = UiFactory.CreateKeyLabel("W"); 
            comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S"); 
            comps.LblD = UiFactory.CreateKeyLabel("D");
            
            // 布局 WASD
            keysGrid.Controls.Add(comps.LblW, 1, 0); // W
            keysGrid.Controls.Add(comps.LblA, 0, 1); // A
            keysGrid.Controls.Add(comps.LblS, 1, 1); // S
            keysGrid.Controls.Add(comps.LblD, 2, 1); // D
            keyContainer.Controls.Add(keysGrid);

            // (3) 按钮面板 (Dock=Bottom)
            comps.PnlButtons = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Bottom, Height = 80, 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = true, 
                Padding = new Padding(20, 5, 0, 0) 
            };
            
            // (4) 日志列表 (Dock=Fill)
            // 填充剩余空间
            comps.ListHistory = UiFactory.CreateLogList();

            // 组装左侧 (注意 Dock 添加顺序，决定了堆叠顺序)
            // Fill 最后添加会填充中间剩余部分
            comps.LeftPanel.Controls.Add(comps.ListHistory); // Fill (中间)
            comps.LeftPanel.Controls.Add(comps.PnlButtons);  // Bottom (底部)
            comps.LeftPanel.Controls.Add(keyContainer);      // Top (第二顶)
            comps.LeftPanel.Controls.Add(comps.PnlBubbleArea); // Top (最顶端)

            mainLayout.Controls.Add(comps.LeftPanel, 0, 0);

            // === 右侧图表构建 ===
            var chartsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            chartsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            chartsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            
            comps.GraphAD = new AdvancedGraph { Dock = DockStyle.Fill }; 
            comps.GraphAD.SetTitle("AD Axis");
            
            comps.GraphWS = new AdvancedGraph { Dock = DockStyle.Fill }; 
            comps.GraphWS.SetTitle("WS Axis");
            
            chartsLayout.Controls.Add(comps.GraphAD, 0, 0); 
            chartsLayout.Controls.Add(comps.GraphWS, 0, 1);
            
            mainLayout.Controls.Add(chartsLayout, 1, 0);
            
            // === 模拟模式覆盖层 (Dock=Fill, 默认隐藏) ===
            comps.PnlSimulation = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(20, 20, 20), 
                Visible = false 
            };

// 居中容器
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
                Text = "WAITING FOR STRAFE...", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 36, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            comps.LblSimResult = new Label 
            { 
                Text = "", 
                ForeColor = Color.LightGray, 
                Font = new Font("Segoe UI", 16), 
                TextAlign = ContentAlignment.TopCenter, 
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            comps.BtnExitSim = UiFactory.CreateButton("退出测试", null);
            comps.BtnExitSim.Anchor = AnchorStyles.None;
            comps.BtnExitSim.Size = new Size(200, 50);

            simCenter.Controls.Add(comps.LblSimStatus, 0, 0);
            simCenter.Controls.Add(comps.LblSimResult, 0, 1);
            simCenter.Controls.Add(comps.BtnExitSim, 0, 2);

            comps.PnlSimulation.Controls.Add(simCenter);
            form.Controls.Add(comps.PnlSimulation); // 添加到最外层，覆盖所有
            comps.PnlSimulation.BringToFront();

            return comps;
        }

        // 添加按钮的方法
        public static void AddButtons(GameUiComponents comps, EventHandler onTest, EventHandler onMap, EventHandler onThres, EventHandler onCount, EventHandler onReset)
        {
            // 创建各个功能按钮
            comps.BtnTest = UiFactory.CreateButton("测试", onTest);
            comps.BtnMap = UiFactory.CreateButton("映射", onMap);
            comps.BtnThreshold = UiFactory.CreateButton("阈值", onThres);
            comps.BtnCount = UiFactory.CreateButton("次数", onCount);
            comps.BtnReset = UiFactory.CreateButton("重置", onReset);
            
            // 添加到流式面板中
            comps.PnlButtons.Controls.Add(comps.BtnTest);
            comps.PnlButtons.Controls.Add(comps.BtnMap);
            comps.PnlButtons.Controls.Add(comps.BtnThreshold);
            comps.PnlButtons.Controls.Add(comps.BtnCount);
            comps.PnlButtons.Controls.Add(comps.BtnReset);
        }
    }
}