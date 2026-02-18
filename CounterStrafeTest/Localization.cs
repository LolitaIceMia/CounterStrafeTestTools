using System.Collections.Generic;

namespace CounterStrafeTest
{
    public enum AppLanguage { Chinese, English }

    public static class Localization
    {
        public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.Chinese;

        private static readonly Dictionary<AppLanguage, Dictionary<string, string>> _resources = new()
        {
            {
                AppLanguage.Chinese, new Dictionary<string, string>
                {
                    { "Startup_Msg", "您使用的是磁轴键盘吗？" },
                    { "Startup_Title", "设备选择" },
                    { "Title", "CS2 急停评估工具 Pro" },
                    { "Feedback_Default", "请进行 AD 或 WS 急停" },
                    
                    // Keys
                    { "Key_W", "W键" }, { "Key_A", "A键" }, { "Key_S", "S键" }, { "Key_D", "D键" },
                    { "Status_Pressed", "按下" }, { "Status_Released", "未按下" },

                    // Charts
                    { "Chart_AD_Title", "AD 急停时间差 (ms)" },
                    { "Chart_WS_Title", "WS 急停时间差 (ms)" },
                    { "Chart_X_Axis", "次数 (Index)" },
                    { "Chart_Y_Axis", "延迟 (ms)" },
                    { "Chart_Avg", "平均值" },

                    // Buttons
                    { "Btn_Refresh", "刷新 (F5)" },
                    { "Btn_Mapping", "按键映射 (F6)" },
                    { "Btn_Threshold", "过滤阈值" },
                    { "Btn_Count", "记录次数" },
                    { "Lang_Switch", "语言/Language" },

                    // Evaluation
                    { "Eval_Perfect", "完美" },
                    { "Eval_Early", "按早了" },
                    { "Eval_Late", "按晚了" },
                    
                    // Messages
                    { "Msg_Mapping", "按键映射设置：\n请依次修改对应关系。" },
                    { "Msg_Threshold", "请输入过滤阈值 (ms, 默认120):" },
                    { "Msg_Count", "请输入图表显示最近记录数 (默认20):" }
                }
            },
            {
                AppLanguage.English, new Dictionary<string, string>
                {
                    { "Startup_Msg", "Are you using a Magnetic Switch keyboard?" },
                    { "Startup_Title", "Device Selection" },
                    { "Title", "CS2 Counter-Strafe Tool Pro" },
                    { "Feedback_Default", "Perform AD or WS Counter-Strafe" },

                    { "Key_W", "Key W" }, { "Key_A", "Key A" }, { "Key_S", "Key S" }, { "Key_D", "Key D" },
                    { "Status_Pressed", "Down" }, { "Status_Released", "Up" },

                    { "Chart_AD_Title", "AD Strafe Timing (ms)" },
                    { "Chart_WS_Title", "WS Strafe Timing (ms)" },
                    { "Chart_X_Axis", "Count" },
                    { "Chart_Y_Axis", "Latency" },
                    { "Chart_Avg", "Average" },

                    { "Btn_Refresh", "Refresh (F5)" },
                    { "Btn_Mapping", "Key Map (F6)" },
                    { "Btn_Threshold", "Threshold" },
                    { "Btn_Count", "Rec Count" },
                    { "Lang_Switch", "Language" },

                    { "Eval_Perfect", "Perfect" },
                    { "Eval_Early", "Too Early" },
                    { "Eval_Late", "Too Late" },

                    { "Msg_Mapping", "Key Mapping Settings:" },
                    { "Msg_Threshold", "Enter Filter Threshold (ms, def 120):" },
                    { "Msg_Count", "Enter Graph Record Limit (def 20):" }
                }
            }
        };

        public static string Get(string key) => _resources[CurrentLanguage].TryGetValue(key, out var v) ? v : key;
    }
}