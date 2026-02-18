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

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            form.Controls.Add(mainLayout);

            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 1. [新增] 气泡专用区域 (Dock=Top)
            // 高度 120px，足以容纳 3 个层叠的气泡 (50 + 15 + 15 = 80px)
            comps.PnlBubbleArea = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 120, 
                BackColor = Color.Transparent 
            };

            // 2. 按键网格 (Dock=Top)
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

            // 3. 按钮面板 (Dock=Bottom)
            comps.PnlButtons = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Bottom, Height = 80, 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = true, Padding = new Padding(20, 5, 0, 0) 
            };
            
            // 4. 日志列表 (Dock=Fill)
            comps.ListHistory = UiFactory.CreateLogList();

            // 组装顺序：Fill -> Bottom -> Top(Key) -> Top(Bubble)
            // 这样 BubbleArea 会在最顶端，KeyContainer 在它下面
            // ListHistory 会自动填充剩余空间 (也就是被BubbleArea挤压了)
            leftPanel.Controls.Add(comps.ListHistory); 
            leftPanel.Controls.Add(comps.PnlButtons);  
            leftPanel.Controls.Add(keyContainer);      
            leftPanel.Controls.Add(comps.PnlBubbleArea); // 最先添加的 Dock=Top 会在最上面

            mainLayout.Controls.Add(leftPanel, 0, 0);

            // ... 右侧图表代码保持不变 ...
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
            
            comps.PnlButtons.Controls.Add(comps.BtnRefresh);
            comps.PnlButtons.Controls.Add(comps.BtnMap);
            comps.PnlButtons.Controls.Add(comps.BtnThreshold);
            comps.PnlButtons.Controls.Add(comps.BtnCount);
            comps.PnlButtons.Controls.Add(comps.BtnReset);
        }
    }
}