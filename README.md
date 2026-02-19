# CS2 Counter-Strafe Tool Pro | CS2 急停评估工具 Pro

---

<a name="english"></a>

## 🇺🇸 English

### Overview

**CS2 Counter-Strafe Tool Pro** is a high-precision analysis software designed for FPS competitive players (specifically *Counter-Strike 2*). Unlike traditional key overlays, this tool uses **High-Resolution Timers (QPC)** and **Raw Input** to capture sub-millisecond keyboard and mouse events.

It is not just a practice tool, but a **Hardware & Skill Diagnostic System**. Its core feature, the **Magnetic Switch Debugger**, utilizes statistical algorithms to calculate the optimal Rapid Trigger (RT) and Deadzone settings tailored to your specific muscle memory.

### 🌟 Key Feature: Precision Magnetic Switch Tuning

Stop guessing whether your RT should be `0.1mm` or `0.3mm`. Let data decide.

The **Magnet Debug** module runs a 30-round extreme counter-strafing test to analyze your physical input habits. Based on the **Mean Latency** and **Standard Deviation (Stability)** of your inputs, it provides scientific parameter recommendations:

* **🎯 RT Actuation/Reset Optimization:**
* Detects **Gap (Too Late)** bias: Suggests decreasing Actuation travel to improve reaction speed.
* Detects **Overlap (Too Early)** bias: Suggests decreasing Reset travel to accelerate key release.


* **📉 Dynamic Deadzone Algorithm:**
* Analyzes signal noise and finger jitter (Chatter).
* Calculates a **"Stability Tax"** to recommend a dynamic **Release Deadzone (Hysteresis)**, eliminating accidental double-inputs while maintaining maximum speed.


* **🚫 Mispress Prevention:**
* Identifies accidental presses during directional changes and recommends an optimal **Press Deadzone**.



### 🚀 Core Functions

1. **Strafe Latency Analysis**
* Real-time monitoring of `AD` / `WS` directional changes.
* Distinguishes between **Overlap** (keys held together) and **Gap** (delay between release and press).
* Visualizes performance with advanced latency distribution graphs.


2. **Shooting Timing Assessment (Sim Mode)**
* Simulates in-game peak/jiggle peeking scenarios.
* Measures the delay between **"Strafe Stop"** and **"Mouse Click"**.
* Ratings: **PERFECT** (<5ms error), **GREAT** (<10ms error), Too Early/Late warnings.


3. **Visualization & History**
* Rolling log of all input events.
* Real-time color-coded feedback bubbles.
* Historical charts to track consistency over time.



### How to Use

1. **Launch**: Run the executable. Select "Yes" if you are using a Magnetic Switch keyboard.
2. **Calibrate (Important)**: Click the **Magnet Debug** button (Gold button). Follow the instructions to perform 30 counter-strafes.
3. **Apply Settings**: View the generated report and apply the recommended **RT** and **Deadzone** settings in your keyboard's driver software.
4. **Practice**: Enter **Sim Test** mode to practice your rhythm with the new settings.

---

<a name="chinese"></a>

## 🇨🇳 简体中文

### 项目简介

**CS2 急停评估工具 Pro** 是一款专为 FPS 竞技玩家（特别是 *Counter-Strike 2*）设计的高精度分析软件。不同于传统的按键显示工具，本项目基于 **高精度计时器 (QPC)** 和 **底层原始输入 (Raw Input)**，能够捕捉亚毫秒级的键鼠事件。

它不仅仅是一个练习软件，更是一个**外设与技术诊断系统**。其核心功能**【磁轴精准调试】**利用数理统计算法，为您量身计算最适合您肌肉记忆的 Rapid Trigger (RT) 和死区设置。

### 🌟 核心亮点：磁轴精准调试 (Magnet Debug)

拒绝玄学调参。不要再盲目模仿职业选手的 `0.1mm` 设置，让数据说话。

**磁轴调试模块**通过 30 轮极限急停测试，深度分析您的物理操作习惯。基于操作延迟的**均值 (偏向性)** 和 **标准差 (稳定性)**，算法将给出科学的参数建议：

* **🎯 RT 行程动态优化：**
* 检测 **Gap (急停过迟)** 倾向：算法将建议减小**快速触发行程 (AP)**，压榨物理触发时间。
* 检测 **Overlap (急停过早)** 倾向：算法将建议减小**快速重置行程 (RT)**，加快松手信号的传输。


* **📉 动态死区算法 (Dynamic Deadzone)：**
* 通过分析操作方差，识别手指抖动和传感器底噪。
* 引入**“稳定性税” (Stability Tax)** 概念，为您计算最佳的**抬起死区 (迟滞)**，在杜绝“断触/连点”的同时保持极致响应。


* **🚫 误触防御机制：**
* 识别高压操作下的误触行为，推荐最佳的**初始按下死区**。



### 🚀 主要功能

1. **急停延迟分析**
* 实时监控 `AD` / `WS` 轴向切换。
* 精准区分 **重叠 (Overlap)** 和 **间隙 (Gap)** 两种失误类型。
* 提供可视化的延迟分布图表，帮助您找到手感重心。


2. **射击时机评估 (模拟模式)**
* 还原游戏内 Peek /急停射击场景。
* 测量 **“急停完成瞬间”** 到 **“鼠标左键触发”** 的微秒级延迟。
* 评级系统：**PERFECT** (误差<5ms)，**GREAT** (误差<10ms)，以及过早/过迟警报。


3. **可视化与历史记录**
* 滚动日志记录所有操作细节。
* 实时气泡反馈（颜色编码）。
* 历史趋势图表，跟踪您的稳定性变化。



### 使用指南

1. **启动**：运行程序。如果是磁轴键盘用户，请在启动弹窗中选择“是”。
2. **校准 (关键步骤)**：点击右上角的 **【磁轴调试】** 按钮（金色）。按照提示完成 30 次急停操作。
3. **应用参数**：根据弹出的分析报告，在您的键盘驱动中填入推荐的 **RT 行程** 和 **死区数值**。
4. **实战练习**：进入 **【模拟测试】** 模式，适应新设置下的急停与开火节奏。

---

> **Note / 注意**:
> This tool analyzes hardware input latency via software hooks. Actual in-game performance may vary depending on network conditions (Ping) and server tick rate.
> 本工具通过软件钩子分析硬件输入延迟。实际游戏表现可能受网络环境 (Ping) 和服务器刷新率 (Tick rate) 影响。
