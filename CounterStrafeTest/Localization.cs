using System.Collections.Generic;

namespace CounterStrafeTest
{
    // 定义支持的语言
    public enum AppLanguage
    {
        Chinese,
        English
    }

    public static class Localization
    {
        // 当前选中的语言，默认中文
        public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.Chinese;

        // 字典结构：语言 -> (键 -> 值)
        private static readonly Dictionary<AppLanguage, Dictionary<string, string>> _resources = new()
        {
            // ================== 中文 (Chinese) ==================
            {
                AppLanguage.Chinese, new Dictionary<string, string>
                {
                    // 启动对话框
                    { "Startup_Msg", "您使用的是磁轴键盘（支持模拟信号/RT功能）吗？\n\n是 (Yes) -> 启用磁轴校准与模拟算法调试模式\n否 (No)  -> 仅启用标准 CS2 急停延迟测量工具" },
                    { "Startup_Title", "设备类型选择" },
                    
                    // 主界面
                    { "Title", "CS2 急停评估工具" },
                    { "Feedback_Default", "请模拟 PEEK 进行 AD 急停" },
                    { "Key_A", "A键" },
                    { "Key_D", "D键" },
                    { "Key_W", "W键" },
                    { "Key_S", "S键" },
                    { "Status_Pressed", "按下" },
                    { "Status_Released", "未按下" },
                    { "Chart_Title", "急停分布图" },
                    { "Lang_Switch", "语言/Language" },

                    // 结果反馈
                    { "Eval_Perfect", "完美" },
                    { "Eval_Early", "按早了" },
                    { "Eval_Late", "按晚了" }
                }
            },

            // ================== 英文 (English) ==================
            {
                AppLanguage.English, new Dictionary<string, string>
                {
                    // Startup Dialog
                    { "Startup_Msg", "Are you using a Magnetic Switch keyboard (Analog/RT supported)?\n\nYes -> Enable Magnetic Calibration & RT Algorithm Mode\nNo  -> Enable Standard CS2 Counter-Strafe Measurement" },
                    { "Startup_Title", "Device Selection" },

                    // Main UI
                    { "Title", "CS2 Counter-Strafe Tool" },
                    { "Feedback_Default", "Perform AD Counter-Strafe (PEEK)" },
                    { "Key_A", "Key A" },
                    { "Key_D", "Key D" },
                    { "Key_W", "Key W" },
                    { "Key_S", "Key S" },
                    { "Status_Pressed", "Down" },
                    { "Status_Released", "Up" },
                    { "Chart_Title", "Distribution Chart" },
                    { "Lang_Switch", "Language" },

                    // Evaluation
                    { "Eval_Perfect", "Perfect" },
                    { "Eval_Early", "Too Early" },
                    { "Eval_Late", "Too Late" }
                }
            }
        };

        // 获取文本的通用方法
        public static string Get(string key)
        {
            if (_resources[CurrentLanguage].TryGetValue(key, out string value))
            {
                return value;
            }
            return $"[{key}]"; // 如果找不到key，返回[key]方便调试
        }
    }
}