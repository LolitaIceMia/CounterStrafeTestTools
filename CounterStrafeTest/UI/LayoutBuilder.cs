using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public class GameUiComponents
    {
        public FlowLayoutPanel PnlBubbles;
        public FlowLayoutPanel PnlButtons; // [新增] 直接引用按钮面板
        
        public Label LblW, LblA, LblS, LblD;
        public ListBox ListHistory;
        public AdvancedGraph GraphAD;
        public AdvancedGraph GraphWS;
        public Button BtnRefresh, BtnMap, BtnThreshold, BtnCount, BtnReset;
    }

    public static class LayoutBuilder
    {
        public static GameUiComponents Build(Form form)
        {
            form.ClientSize = new Size(1280, 720);
            form.MinimumSize = new Size(1280, 720);
            form.StartPosition = FormStartPosition.CenterScreen;

            var comps = new GameUiComponents();

            // 主容器
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            form.Controls.Add(mainLayout);

            // === 左侧 ===
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 1. 气泡容器
            comps.PnlBubbles = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(25, 10, 0, 20),
                BackColor = Color.Transparent
            };

            // 2. 按键网格
            var keyContainer = new Panel { Dock = DockStyle.Top, Height = 180 }; 
            var keysGrid = new TableLayoutPanel 
            { 
                Width = 260, Height = 170, 
                ColumnCount = 3, RowCount = 2,
                Left = (450 - 260) / 2 - 10, Top = 5 
            };
            comps.LblW = UiFactory.CreateKeyLabel("W"); comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S"); comps.LblD = UiFactory.CreateKeyLabel("D");
            keysGrid.Controls.Add(comps.LblW, 1, 0); keysGrid.Controls.Add(comps.LblA, 0, 1);
            keysGrid.Controls.Add(comps.LblS, 1, 1); keysGrid.Controls.Add(comps.LblD, 2, 1);
            keyContainer.Controls.Add(keysGrid);

            // 3. 按钮面板 [修改：赋值给 comps.PnlButtons]
            comps.PnlButtons = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Bottom, 
                Height = 80, 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = true, 
                Padding = new Padding(20, 5, 0, 0) 
            };
            
            // 4. 日志列表
            comps.ListHistory = UiFactory.CreateLogList();

            // 组装左侧
            leftPanel.Controls.Add(comps.ListHistory); // Fill
            leftPanel.Controls.Add(comps.PnlButtons);  // Bottom
            leftPanel.Controls.Add(keyContainer);      // Top
            leftPanel.Controls.Add(comps.PnlBubbles);  // Top (最上)

            mainLayout.Controls.Add(leftPanel, 0, 0);

            // === 右侧图表 ===
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

            return comps;
        }
        
        public static void AddButtons(GameUiComponents comps, EventHandler onRefresh, EventHandler onMap, EventHandler onThres, EventHandler onCount, EventHandler onReset)
        {
            comps.BtnRefresh = UiFactory.CreateButton("刷新", onRefresh);
            comps.BtnMap = UiFactory.CreateButton("映射", onMap);
            comps.BtnThreshold = UiFactory.CreateButton("阈值", onThres);
            comps.BtnCount = UiFactory.CreateButton("次数", onCount);
            comps.BtnReset = UiFactory.CreateButton("重置", onReset);
            
            // 直接添加到刚才保存的 Panel 中
            comps.PnlButtons.Controls.Add(comps.BtnRefresh);
            comps.PnlButtons.Controls.Add(comps.BtnMap);
            comps.PnlButtons.Controls.Add(comps.BtnThreshold);
            comps.PnlButtons.Controls.Add(comps.BtnCount);
            comps.PnlButtons.Controls.Add(comps.BtnReset);
        }
    }
}