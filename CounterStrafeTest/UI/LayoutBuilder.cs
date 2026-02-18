using System;
using System.Windows.Forms;
using CounterStrafeTest.Utils; // 引用工具包

namespace CounterStrafeTest.UI
{
    // UI 组件容器，方便 MainForm 访问控件
    public class GameUiComponents
    {
        public Label LblFeedback;
        public Label LblW, LblA, LblS, LblD;
        public ListBox ListHistory;
        public AdvancedGraph GraphAD;
        public AdvancedGraph GraphWS;
        public Button BtnRefresh, BtnMap, BtnThreshold, BtnCount;
    }

    public static class LayoutBuilder
    {
        public static GameUiComponents Build(Form form)
        {
            var comps = new GameUiComponents();
            form.ClientSize = new Size(1280, 720);
            form.MinimumSize = new Size(1280, 720); // 防止缩得太小
            form.StartPosition = FormStartPosition.CenterScreen;
            // 1. 主容器 (上下结构)
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            form.Controls.Add(mainLayout);

            // 2. 顶部反馈
            comps.LblFeedback = UiFactory.CreateTitleLabel("Ready");
            mainLayout.Controls.Add(comps.LblFeedback, 0, 0);

            // 3. 内容区 (左右结构)
            var contentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(contentLayout, 0, 1);

            // === 左侧 ===
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 按键网格
            var keysGrid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 120, ColumnCount = 3, RowCount = 2 };
            comps.LblW = UiFactory.CreateKeyLabel("W"); comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S"); comps.LblD = UiFactory.CreateKeyLabel("D");
            keysGrid.Controls.Add(comps.LblW, 1, 0);
            keysGrid.Controls.Add(comps.LblA, 0, 1);
            keysGrid.Controls.Add(comps.LblS, 1, 1);
            keysGrid.Controls.Add(comps.LblD, 2, 1);

            // 按钮面板
            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60 };
            
            
            // 日志列表
            comps.ListHistory = UiFactory.CreateLogList();

            leftPanel.Controls.Add(comps.ListHistory);
            leftPanel.Controls.Add(btnPanel);
            leftPanel.Controls.Add(keysGrid);
            contentLayout.Controls.Add(leftPanel, 0, 0);

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
            contentLayout.Controls.Add(chartsLayout, 1, 0);

            return comps;
        }
        
        // 辅助方法：向面板添加按钮并绑定
        public static void AddButtons(GameUiComponents comps, FlowLayoutPanel panel, 
            EventHandler onRefresh, EventHandler onMap, EventHandler onThres, EventHandler onCount)
        {
            comps.BtnRefresh = UiFactory.CreateButton("刷新 (F5)", onRefresh);
            comps.BtnMap = UiFactory.CreateButton("按键映射 (F6)", onMap);
            comps.BtnThreshold = UiFactory.CreateButton("阈值", onThres);
            comps.BtnCount = UiFactory.CreateButton("记录数", onCount);
            
            panel.Controls.Add(comps.BtnRefresh);
            panel.Controls.Add(comps.BtnMap);
            panel.Controls.Add(comps.BtnThreshold);
            panel.Controls.Add(comps.BtnCount);
        }
    }
}