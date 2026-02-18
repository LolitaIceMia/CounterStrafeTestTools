using System.Collections.Generic;

namespace CounterStrafeTest
{
    /// <summary>
    /// 应用程序支持的语言枚举
    /// </summary>
    public enum AppLanguage { Chinese, English }

    /// <summary>
    /// 提供全局多语言支持的静态类
    /// </summary>
    public static class Localization
    {
        // 默认为中文
        public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.Chinese;

        private static readonly Dictionary<AppLanguage, Dictionary<string, string>> _resources = new()
        {
            {
                AppLanguage.Chinese, new Dictionary<string, string>
                {
                    { "Title", "CS2 急停评估工具 Pro From 狐瓜西" },
                    { "Btn_Test", "模拟测试" },
                    { "Btn_Mapping", "按键映射" },
                    { "Btn_Threshold", "判定阈值" },
                    { "Btn_Count", "样本上限" },
                    { "Btn_Reset", "重置数据" },
                    { "Btn_Exit_Sim", "退出测试" },
                    
                    // 状态与按键
                    { "Status_Pressed", "按下" },
                    { "Status_Released", "松开" },
                    { "Eval_Perfect", "完美" },
                    { "Eval_Early", "过早" },
                    { "Eval_Late", "过迟" },

                    // 模拟模式 UI
                    { "Sim_Ready", "准备就绪：请执行急停 + 开火" },
                    { "Sim_Waiting", "等待操作..." },
                    { "Sim_Reset_Tip", "2秒后系统自动重置..." },
                    
                    // 判定等级
                    { "Grade_Early_Strafe_Overlap", "急停过早 (重叠过长)" },
                    { "Grade_Late_Strafe_Slow", "急停过迟 (松键太慢)" },
                    { "Grade_Early_Fire", "开火过早" },
                    { "Grade_Late_Fire", "开火过迟" },
                    { "Grade_Perfect", "PERFECT (完美)" },
                    { "Grade_Great", "GREAT (优秀)" },

                    // 详细提示 (用于逻辑分析)
                    { "Tip_Strafe_Fail", "急停物理延迟超过 15ms，不合格" },
                    { "Tip_Fire_Early", "你在急停完成前过早开火" },
                    { "Tip_Fire_Late", "急停完成后等待时间过长" },
                    { "Tip_Perfect", "教科书级的急停射击！" },
                    { "Tip_Great", "非常出色的操作配合" },

                    // 交互弹窗
                    { "Dialog_Reset_Confirm", "确认重置所有数据？" },
                    { "Dialog_Reset_Title", "重置确认" },
                    { "Dialog_Map_Title", "按键映射" },
                    { "Dialog_Map_Msg", "请输入4个逻辑键字母 (WASD):" },
                    { "Dialog_Thres_Title", "阈值设置" },
                    { "Dialog_Thres_Msg", "输入判定阈值 (ms):" },
                    { "Dialog_Count_Title", "容量设置" },
                    { "Dialog_Count_Msg", "设置图表显示的最大样本数:" },

                    // 图表标题
                    { "Chart_AD_Title", "AD 急停时间差 (ms)" },
                    { "Chart_WS_Title", "WS 急停时间差 (ms)" }
                }
            },
            {
                AppLanguage.English, new Dictionary<string, string>
                {
                    { "Title", "CS2 Counter-Strafe Pro" },
                    { "Btn_Test", "Sim Test" },
                    { "Btn_Mapping", "Key Map" },
                    { "Btn_Threshold", "Threshold" },
                    { "Btn_Count", "Capacity" },
                    { "Btn_Reset", "Reset All" },
                    { "Btn_Exit_Sim", "Exit Sim" },

                    // Status & Keys
                    { "Status_Pressed", "Down" },
                    { "Status_Released", "Up" },
                    { "Eval_Perfect", "Perfect" },
                    { "Eval_Early", "Early" },
                    { "Eval_Late", "Late" },

                    // Sim Mode UI
                    { "Sim_Ready", "Ready: Perform Strafe + Fire" },
                    { "Sim_Waiting", "Waiting..." },
                    { "Sim_Reset_Tip", "Resetting in 2s..." },

                    // Grade Results
                    { "Grade_Early_Strafe_Overlap", "Too Early (Overlap)" },
                    { "Grade_Late_Strafe_Slow", "Too Late (Slow Release)" },
                    { "Grade_Early_Fire", "Fire Early" },
                    { "Grade_Late_Fire", "Fire Late" },
                    { "Grade_Perfect", "PERFECT" },
                    { "Grade_Great", "GREAT" },

                    // Feedback Tips
                    { "Tip_Strafe_Fail", "Physical latency > 15ms, invalid" },
                    { "Tip_Fire_Early", "Fired before strafe completed" },
                    { "Tip_Fire_Late", "Too much delay after stopping" },
                    { "Tip_Perfect", "Textbook counter-strafe shot!" },
                    { "Tip_Great", "Excellent coordination" },

                    // Dialogs
                    { "Dialog_Reset_Confirm", "Are you sure to reset all data?" },
                    { "Dialog_Reset_Title", "Reset" },
                    { "Dialog_Map_Title", "Mapping" },
                    { "Dialog_Map_Msg", "Enter 4 logical keys (WASD):" },
                    { "Dialog_Thres_Title", "Threshold" },
                    { "Dialog_Thres_Msg", "Enter threshold (ms):" },
                    { "Dialog_Count_Title", "Capacity" },
                    { "Dialog_Count_Msg", "Set max chart samples:" },

                    // Charts
                    { "Chart_AD_Title", "AD Strafe Timing (ms)" },
                    { "Chart_WS_Title", "WS Strafe Timing (ms)" }
                }
            }
        };

        /// <summary>
        /// 根据 Key 获取当前语言对应的文本
        /// </summary>
        public static string Get(string key) 
        {
            if (_resources.TryGetValue(CurrentLanguage, out var dict))
            {
                if (dict.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            return key; // 如果找不到，返回 Key 原文作为降级处理
        }
    }
}