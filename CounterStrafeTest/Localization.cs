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
                    // === 启动与主界面 ===
                    { "Startup_Title", "设备选择" },
                    { "Startup_Msg", "您使用的键盘轴体是否为磁轴？" },
                    { "Title", "CS2 急停评估工具 Pro" },
                    { "Btn_Test", "模拟测试" },
                    { "Btn_Mapping", "按键映射" },
                    { "Btn_Threshold", "判定阈值" },
                    { "Btn_Count", "样本上限" },
                    { "Btn_Reset", "重置数据" },
                    { "Btn_Exit_Sim", "退出测试" },
                    { "Btn_MagnetDebug", "磁轴调试" },
                    
                    // === 状态与按键 ===
                    { "Status_Pressed", "按下" },
                    { "Status_Released", "松开" },
                    { "Eval_Perfect", "完美" },
                    { "Eval_Early", "过早" },
                    { "Eval_Late", "过迟" },

                    // === 模拟模式 UI ===
                    { "Item_1", "急停延迟" },
                    { "Item_2", "开火延迟" },
                    { "Sim_Ready", "准备就绪：请执行急停 + 开火" },
                    { "Sim_Waiting", "等待操作..." },
                    { "Sim_Reset_Tip", "2秒后系统自动重置..." },
                    
                    // === 判定等级 ===
                    { "Grade_Early_Strafe_Overlap", "急停过早" },
                    { "Grade_Late_Strafe_Slow", "急停过迟 " },
                    { "Grade_Early_Fire", "开火过早" },
                    { "Grade_Late_Fire", "开火过迟" },
                    { "Grade_Perfect", "PERFECT (完美)" },
                    { "Grade_Great", "GREAT (优秀)" },

                    // === 详细提示 ===
                    { "Tip_Strafe_Fail", "急停物理延迟超过 15ms，不合格" },
                    { "Tip_Fire_Early", "你在急停完成前过早开火" },
                    { "Tip_Fire_Late", "急停完成后等待时间过长" },
                    { "Tip_Perfect", "教科书级的急停射击！" },
                    { "Tip_Great", "非常出色的操作配合" },

                    // === 交互弹窗 ===
                    { "Dialog_Reset_Confirm", "确认重置所有数据？" },
                    { "Dialog_Reset_Title", "重置确认" },
                    { "Dialog_Map_Title", "按键映射" },
                    { "Dialog_Map_Msg", "请输入4个逻辑键字母 (WASD):" },
                    { "Dialog_Thres_Title", "阈值设置" },
                    { "Dialog_Thres_Msg", "输入判定阈值 (ms):" },
                    { "Dialog_Count_Title", "容量设置" },
                    { "Dialog_Count_Msg", "设置图表显示的最大样本数:" },

                    // === 图表标题 ===
                    { "Chart_AD_Title", "AD 急停时间差 (ms)" },
                    { "Chart_WS_Title", "WS 急停时间差 (ms)" },

                    // === 磁轴调试 (Magnet Debug) ===
                    { "Title_MagnetDebug", "磁轴调试页面" },
                    { "Btn_Start_Test", "开始测试并调节" },
                    { "Btn_Stop", "停止测试" },
                    { "Btn_Magnet_Settings", "当前键盘参数设置" },
                    { "Magnet_Report_Title", "磁轴校准报告" },
                    
                    // 磁轴参数设置弹窗
                    { "Dialog_Settings_Title", "磁轴参数设置" },
                    { "Label_RtActuation", "快速触发 (RT Actuation) [mm]" },
                    { "Label_RtReset", "快速重置 (RT Reset) [mm]" },
                    { "Label_PressDz", "死区按下 (Press Deadzone) [mm]" },
                    { "Label_ReleaseDz", "死区抬起 (Release/Hysteresis) [mm]" },

                    // 磁轴分析报告
                    { "Mag_Report_Mean", "平均延迟" },
                    { "Mag_Report_StdDev", "稳定性(标准差)" },
                    { "Mag_Report_Rec", "--- 调节建议 ---" },
                    { "Mag_Report_Settings", "推荐设置" },
                    { "Mag_Setting_AP", "触发行程 (AP)" },
                    { "Mag_Setting_RT", "重置行程 (RT)" },
                    { "Mag_Setting_DZ", "死区 (Deadzone)" },

                    // 磁轴算法分析文本
                    { "Mag_Analysis_Early", "检测到急停倾向过早 (Overlap)" },
                    { "Mag_Rec_Decrease_Reset", "建议：减小快速重置行程 (RT Reset)，加快松键响应。" },

                    { "Mag_Analysis_Late", "检测到急停倾向过迟 (Gap)" },
                    { "Mag_Rec_Decrease_Actuation", "建议：减小快速触发行程 (RT Actuation)，加快触发响应。" },

                    { "Mag_Analysis_Good", "急停时机控制良好，保持当前行程设置。" },

                    { "Mag_Analysis_Unstable", "检测到信号颤噪或操作抖动 (High Variance)" },
                    { "Mag_Rec_Increase_Deadzone", "建议：增加动态死区 (Dynamic Hysteresis) 以过滤噪声。" },

                    { "Mag_Analysis_Mispress", "检测到可能的误触 (Accidental Press)" },
                    { "Mag_Rec_Press_Deadzone", "建议：增大初始按下死区 (Press Deadzone)。" }
                }
            },
            {
                AppLanguage.English, new Dictionary<string, string>
                {
                    // === Startup & Main UI ===
                    { "Startup_Title", "Device Selection" },
                    { "Startup_Msg", "Are you using a Magnetic Switch keyboard?" },
                    { "Title", "CS2 Counter-Strafe Pro" },
                    { "Btn_Test", "Sim Test" },
                    { "Btn_Mapping", "Key Map" },
                    { "Btn_Threshold", "Threshold" },
                    { "Btn_Count", "Capacity" },
                    { "Btn_Reset", "Reset All" },
                    { "Btn_Exit_Sim", "Exit Sim" },
                    { "Btn_MagnetDebug", "Magnet Debug" },

                    // === Status & Keys ===
                    { "Status_Pressed", "Down" },
                    { "Status_Released", "Up" },
                    { "Eval_Perfect", "Perfect" },
                    { "Eval_Early", "Early" },
                    { "Eval_Late", "Late" },

                    // === Sim Mode UI ===
                    { "Item_1", "Strafe Latency" },
                    { "Item_2", "Shoot Delay" },
                    { "Sim_Ready", "Ready: Perform Strafe + Fire" },
                    { "Sim_Waiting", "Waiting..." },
                    { "Sim_Reset_Tip", "Resetting in 2s..." },

                    // === Grade Results ===
                    { "Grade_Early_Strafe_Overlap", "Too Early (Overlap)" },
                    { "Grade_Late_Strafe_Slow", "Too Late (Slow Release)" },
                    { "Grade_Early_Fire", "Fire Early" },
                    { "Grade_Late_Fire", "Fire Late" },
                    { "Grade_Perfect", "PERFECT" },
                    { "Grade_Great", "GREAT" },

                    // === Feedback Tips ===
                    { "Tip_Strafe_Fail", "Physical latency > 15ms, invalid" },
                    { "Tip_Fire_Early", "Fired before strafe completed" },
                    { "Tip_Fire_Late", "Too much delay after stopping" },
                    { "Tip_Perfect", "Textbook counter-strafe shot!" },
                    { "Tip_Great", "Excellent coordination" },

                    // === Dialogs ===
                    { "Dialog_Reset_Confirm", "Are you sure to reset all data?" },
                    { "Dialog_Reset_Title", "Reset" },
                    { "Dialog_Map_Title", "Mapping" },
                    { "Dialog_Map_Msg", "Enter 4 logical keys (WASD):" },
                    { "Dialog_Thres_Title", "Threshold" },
                    { "Dialog_Thres_Msg", "Enter threshold (ms):" },
                    { "Dialog_Count_Title", "Capacity" },
                    { "Dialog_Count_Msg", "Set max chart samples:" },

                    // === Charts ===
                    { "Chart_AD_Title", "AD Strafe Timing (ms)" },
                    { "Chart_WS_Title", "WS Strafe Timing (ms)" },

                    // === Magnet Debug ===
                    { "Title_MagnetDebug", "Magnetic Switch Debug" },
                    { "Btn_Start_Test", "Start Calibration" },
                    { "Btn_Stop", "Stop Test" },
                    { "Btn_Magnet_Settings", "Current Keyboard Settings" },
                    { "Magnet_Report_Title", "Magnetic Calibration Report" },

                    // Settings Dialog
                    { "Dialog_Settings_Title", "Magnetic Settings" },
                    { "Label_RtActuation", "RT Actuation [mm]" },
                    { "Label_RtReset", "RT Reset [mm]" },
                    { "Label_PressDz", "Press Deadzone [mm]" },
                    { "Label_ReleaseDz", "Release Deadzone [mm]" },

                    // Analysis Report
                    { "Mag_Report_Mean", "Mean Latency" },
                    { "Mag_Report_StdDev", "Stability (StdDev)" },
                    { "Mag_Report_Rec", "--- Recommendations ---" },
                    { "Mag_Report_Settings", "Recommended Settings" },
                    { "Mag_Setting_AP", "Actuation (AP)" },
                    { "Mag_Setting_RT", "Rapid Trigger (RT)" },
                    { "Mag_Setting_DZ", "Deadzone" },

                    // Analysis Text
                    { "Mag_Analysis_Early", "Tendency: Too Early (Overlap)" },
                    { "Mag_Rec_Decrease_Reset", "Advice: Decrease RT Reset travel for faster release." },

                    { "Mag_Analysis_Late", "Tendency: Too Late (Gap)" },
                    { "Mag_Rec_Decrease_Actuation", "Advice: Decrease RT Actuation travel for faster press." },

                    { "Mag_Analysis_Good", "Timing is good. Keep current settings." },

                    { "Mag_Analysis_Unstable", "Detected signal chatter or instability" },
                    { "Mag_Rec_Increase_Deadzone", "Advice: Increase Dynamic Hysteresis (Deadzone) to filter noise." },

                    { "Mag_Analysis_Mispress", "Detected potential accidental presses" },
                    { "Mag_Rec_Press_Deadzone", "Advice: Increase initial Press Deadzone." }
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