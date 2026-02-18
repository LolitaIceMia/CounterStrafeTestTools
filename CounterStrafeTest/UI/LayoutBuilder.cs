using System;
using System.Drawing;
using System.Windows.Forms;
using CounterStrafeTest.Utils;

namespace CounterStrafeTest.UI
{
    public class GameUiComponents
    {
        public Label LblFeedback;
        public Label LblW, LblA, LblS, LblD;
        public ListBox ListHistory;
        public AdvancedGraph GraphAD;
        public AdvancedGraph GraphWS;
        public Button BtnRefresh, BtnMap, BtnThreshold, BtnCount, BtnReset; // 新增 BtnReset
    }

    public static class LayoutBuilder
    {
        public static GameUiComponents Build(Form form)
        {
            // 设置窗口大小 (1280x720)
            form.ClientSize = new Size(1280, 720);
            form.MinimumSize = new Size(1280, 720);
            form.StartPosition = FormStartPosition.CenterScreen;

            var comps = new GameUiComponents();

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
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F)); //稍微加宽一点左侧，防止按钮挤压
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(contentLayout, 0, 1);

            // === 左侧 ===
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // --- 3.1 按键网格 (居中优化) ---
            // 我们使用一个 Panel 包裹 TableLayoutPanel 来实现居中
            var keyContainer = new Panel { Dock = DockStyle.Top, Height = 180 }; 
            
            // 实际的网格
            var keysGrid = new TableLayoutPanel 
            { 
                Width = 260, Height = 170, // 3列 * (75+间距) ≈ 260
                ColumnCount = 3, RowCount = 2,
                Left = (450 - 260) / 2 - 10, // 手动水平居中 (Panel宽 - Grid宽)/2 - Padding
                Top = 10 
            };
            
            comps.LblW = UiFactory.CreateKeyLabel("W"); comps.LblA = UiFactory.CreateKeyLabel("A");
            comps.LblS = UiFactory.CreateKeyLabel("S"); comps.LblD = UiFactory.CreateKeyLabel("D");
            
            keysGrid.Controls.Add(comps.LblW, 1, 0);
            keysGrid.Controls.Add(comps.LblA, 0, 1);
            keysGrid.Controls.Add(comps.LblS, 1, 1);
            keysGrid.Controls.Add(comps.LblD, 2, 1);
            
            keyContainer.Controls.Add(keysGrid);

            // --- 3.2 按钮面板 (居中优化) ---
            var btnPanel = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Bottom, 
                Height = 80, 
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20, 5, 0, 0) // 增加左内边距来微调居中感
            };
            // 实际上 FlowLayoutPanel 很难自动居中内容，我们通过增加 Padding 或者让它宽度适应
            // 这里我们采用简单策略：让按钮在底部排列，稍微居中一点
            
            // --- 3.3 日志列表 ---
            comps.ListHistory = UiFactory.CreateLogList();

            // 组装左侧 (注意顺序: Dock Fill 最后加)
            leftPanel.Controls.Add(comps.ListHistory); // Fill
            leftPanel.Controls.Add(btnPanel);          // Bottom
            leftPanel.Controls.Add(keyContainer);      // Top
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
        
        public static void AddButtons(GameUiComponents comps, FlowLayoutPanel panel, 
            EventHandler onRefresh, EventHandler onMap, EventHandler onThres, EventHandler onCount, EventHandler onReset)
        {
            comps.BtnRefresh = UiFactory.CreateButton("刷新", onRefresh);
            comps.BtnMap = UiFactory.CreateButton("映射", onMap);
            comps.BtnThreshold = UiFactory.CreateButton("阈值", onThres);
            comps.BtnCount = UiFactory.CreateButton("次数", onCount);
            comps.BtnReset = UiFactory.CreateButton("重置", onReset); // 重置按钮
            
            // 简单的布局调整，让5个按钮分两行或者挤一挤
            panel.Controls.Add(comps.BtnRefresh);
            panel.Controls.Add(comps.BtnMap);
            panel.Controls.Add(comps.BtnThreshold);
            panel.Controls.Add(comps.BtnCount);
            panel.Controls.Add(comps.BtnReset);
        }
    }
}