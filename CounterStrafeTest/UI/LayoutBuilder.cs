using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public class GameUiComponents
    {
        public Panel PnlBubbleArea;
        public FlowLayoutPanel PnlButtons;
        public Label LblW, LblA, LblS, LblD;
        public ListBox ListHistory;
        public AdvancedGraph GraphAD, GraphWS;
        public Button BtnRefresh, BtnMap, BtnThreshold, BtnCount, BtnReset;
        public Panel LeftPanel;
    }

    public static class LayoutBuilder
    {
        public static GameUiComponents Build(Form form)
        {
            form.ClientSize = new Size(1280, 720);
            form.StartPosition = FormStartPosition.CenterScreen;

            var comps = new GameUiComponents();

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            form.Controls.Add(mainLayout);

            comps.LeftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) }; // Padding清零，方便手动控制

            // 1. 气泡区域 (Top)
            // 高度 = 20(TopMargin) + 50(BubbleHeight) + 20(BottomMargin) = 90
            comps.PnlBubbleArea = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 90, 
                BackColor = Color.Transparent 
            };

            // 2. 按键网格 (Top)
            // 位于气泡区域下方
            var keyContainer = new Panel { Dock = DockStyle.Top, Height = 180 }; 
            var keysGrid = new TableLayoutPanel 
            { 
                Width = 260, Height = 170, 
                ColumnCount = 3, RowCount = 2,
                // 居中显示: (450 - 260) / 2 = 95
                Left = 95, 
                Top = 0 // 紧贴 keyContainer 顶部，因为 PnlBubbleArea 已经留了间隔
            };
            comps.LblW = UiFactory.CreateKeyLabel("W"); comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S"); comps.LblD = UiFactory.CreateKeyLabel("D");
            keysGrid.Controls.Add(comps.LblW, 1, 0); keysGrid.Controls.Add(comps.LblA, 0, 1);
            keysGrid.Controls.Add(comps.LblS, 1, 1); keysGrid.Controls.Add(comps.LblD, 2, 1);
            keyContainer.Controls.Add(keysGrid);

            // 3. 按钮面板 (Bottom)
            comps.PnlButtons = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Bottom, Height = 80, 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = true, Padding = new Padding(20, 5, 0, 0) 
            };
            
            // 4. 日志列表 (Fill)
            comps.ListHistory = UiFactory.CreateLogList();

            // 组装
            comps.LeftPanel.Controls.Add(comps.ListHistory); // Fill
            comps.LeftPanel.Controls.Add(comps.PnlButtons);  // Bottom
            comps.LeftPanel.Controls.Add(keyContainer);      // Top (Second)
            comps.LeftPanel.Controls.Add(comps.PnlBubbleArea); // Top (First)

            mainLayout.Controls.Add(comps.LeftPanel, 0, 0);

            // ... 右侧代码不变 ...
            var chartsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            chartsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            chartsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            comps.GraphAD = new AdvancedGraph { Dock = DockStyle.Fill }; comps.GraphAD.SetTitle("AD Axis");
            comps.GraphWS = new AdvancedGraph { Dock = DockStyle.Fill }; comps.GraphWS.SetTitle("WS Axis");
            chartsLayout.Controls.Add(comps.GraphAD, 0, 0); chartsLayout.Controls.Add(comps.GraphWS, 0, 1);
            mainLayout.Controls.Add(chartsLayout, 1, 0);

            return comps;
        }

        public static void AddButtons(GameUiComponents comps, EventHandler onRefresh, EventHandler onMap, EventHandler onThres, EventHandler onCount, EventHandler onReset)
        {
            comps.BtnRefresh = UiFactory.CreateButton("刷新", onRefresh);
            comps.BtnMap = UiFactory.CreateButton("映射", onMap);
            comps.BtnThreshold = UiFactory.CreateButton("阈值", onThres);
            comps.BtnCount = UiFactory.CreateButton("次数", onCount);
            comps.BtnReset = UiFactory.CreateButton("重置", onReset);
            comps.PnlButtons.Controls.Add(comps.BtnRefresh); comps.PnlButtons.Controls.Add(comps.BtnMap);
            comps.PnlButtons.Controls.Add(comps.BtnThreshold); comps.PnlButtons.Controls.Add(comps.BtnCount);
            comps.PnlButtons.Controls.Add(comps.BtnReset);
        }
    }
}