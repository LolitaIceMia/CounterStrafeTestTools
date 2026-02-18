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
                    
                    { "Key_W", "W" }, { "Key_A", "A" }, { "Key_S", "S" }, { "Key_D", "D" },
                    { "Status_Pressed", "按下" }, { "Status_Released", "松开" },

                    { "Chart_AD_Title", "AD 急停时间差 (ms)" },
                    { "Chart_WS_Title", "WS 急停时间差 (ms)" },

                    { "Btn_Refresh", "刷新 (F5)" },
                    { "Btn_Mapping", "按键映射 (F6)" },
                    { "Btn_Threshold", "过滤阈值" },
                    { "Btn_Count", "记录次数" },
                    { "Btn_Reset", "重置所有" }, // 新增
                    { "Lang_Switch", "语言/Language" },

                    { "Eval_Perfect", "完美" },
                    { "Eval_Early", "按早了" },
                    { "Eval_Late", "按晚了" }
                }
            },
            {
                AppLanguage.English, new Dictionary<string, string>
                {
                    { "Startup_Msg", "Are you using a Magnetic Switch keyboard?" },
                    { "Startup_Title", "Device Selection" },
                    { "Title", "CS2 Counter-Strafe Tool Pro" },
                    { "Feedback_Default", "Perform AD or WS Counter-Strafe" },

                    { "Key_W", "W" }, { "Key_A", "A" }, { "Key_S", "S" }, { "Key_D", "D" },
                    { "Status_Pressed", "Down" }, { "Status_Released", "Up" },

                    { "Chart_AD_Title", "AD Strafe Timing (ms)" },
                    { "Chart_WS_Title", "WS Strafe Timing (ms)" },

                    { "Btn_Refresh", "Refresh (F5)" },
                    { "Btn_Mapping", "Key Map (F6)" },
                    { "Btn_Threshold", "Threshold" },
                    { "Btn_Count", "Rec Count" },
                    { "Btn_Reset", "Reset All" }, // New
                    { "Lang_Switch", "Language" },

                    { "Eval_Perfect", "Perfect" },
                    { "Eval_Early", "Too Early" },
                    { "Eval_Late", "Too Late" }
                }
            }
        };

        public static string Get(string key) => _resources[CurrentLanguage].TryGetValue(key, out var v) ? v : key;
    }
}