# 桌面宠物 (DesktopPet)

一个基于WPF开发的Windows桌面宠物应用，具有情感系统和丰富的互动功能。

## 功能特点

- 🎭 **丰富的情感表现**: 包括开心、兴奋、好奇、困倦等8种情感状态
- 🔄 **动态情感系统**: 根据互动频率和时间自动变化情感状态
- 🌙 **环境感知**: 支持昼夜节律，夜间自动调整行为模式
- ⚙️ **高度可定制**: 可调节情感参数、动画间隔等各项设置
- 📌 **固定显示模式**: 支持选择固定的动画序列循环播放
- 🚀 **开机自启**: 可选择开机自动启动

## 快速开始

### 安装要求
- Windows 10或更高版本
- [.NET 9.0](https://dotnet.microsoft.com/download) 或更高版本 

### 安装方法

#### 方法一：直接运行（推荐）
1. 下载 `DesktopPet-vX.X.X-full.zip`（其中X.X.X为版本号） （若你有.NET 9.0环境则下载`DesktopPet-vX.X.X-framework.zip`
2. 解压到任意目录（如 `D:\Programs\DesktopPet`）
3. 运行 `DesktopPet.exe`
4. （可选）右键菜单中选择"开机自启动"

#### 方法二：从源代码编译
1. 克隆仓库：`git clone https://github.com/vangee0528/DesktopPet/`
2. 安装 .NET 9.0 SDK
3. 在项目目录下执行：
```bash
powershell -ExecutionPolicy ByPass -File ".\scripts\publish.ps1"
```
4. 生成的程序在 `Release`文件夹下

### 基本使用

#### 基础操作
- **移动**: 左键拖拽宠物到任意位置
- **互动**: 左键点击抚摸宠物（增加情感能量）
- **菜单**: 右键点击打开设置菜单
- **退出**: 右键菜单中选择"退出"

#### 设置选项说明
1. **基础设置**
   - 开机自启动：设置是否随Windows启动
   - 固定显示模式：选择固定的动画序列
   - 显示调试窗口：查看宠物当前状态

2. **自定义设置**（右键菜单-设置）
   - 动画间隔：控制不同情绪状态下的动画切换速度
   - 情感参数：调整宠物的情感变化速度
   - 夜间模式：设置夜间行为模式的时间范围

#### 使用技巧
1. **互动建议**
   - 定期抚摸宠物以保持其活力
   - 避免频繁快速点击（会触发生气情绪）
   - 注意观察宠物的情绪变化

2. **个性化设置**
   - 可以通过设置调整宠物的活跃程度
   - 使用固定显示模式可以展示特定的动画序列
   - 夜间模式可以让宠物在指定时间更安静

3. **故障排除**
   - 如果宠物没有反应，可以通过任务管理器关闭后重新启动
   - 如果动画显示异常，检查dog_gifs目录是否完整

### 文件结构

```cpp
DesktopPet/
├── Base/                   # 应用程序基础文件
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── AssemblyInfo.cs
├── Models/                 # 模型类
│   ├── EmotionState.cs
│   ├── PetBehavior.cs
│   ├── PetSettings.cs
│   └── StartupManager.cs
├── Views/                  # 视图文件
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── SettingsWindow.xaml
│   ├── SettingsWindow.xaml.cs
│   ├── FixedModeWindow.xaml
│   └── FixedModeWindow.xaml.cs
├── dog_gifs/              # 资源文件
│   ├── angry/
│   ├── curious/
│   ├── ...
├── scripts/               # 开发工具（不会被发布）
│   ├── emotion_classifier.py   # 表情分类工具
│   ├── gif_classifier.py      # GIF分类工具
│   ├── gif_resizer.py        # GIF大小调整工具
│   └── publish.ps1           # 一键发布脚本
├── version.json           # 版本管理文件
├── dog_gifs_backup.zip    # 备份文件
├── README.md              # 项目说明
└── DesktopPet.csproj      # 项目文件
```


### 情感系统设计

#### 核心情感驱动因素
1. **情感能量（Emotion Energy）**: 0-100
   - 代表宠物当前的情感活力水平
   - 随时间自然衰减（每分钟-1点）
   - 被抚摸时增加（每次+15点）
   - 影响：高能量时更容易表现积极情绪

2. **互动渴望（Interaction Need）**: 0-100
   - 代表宠物希望被关注的渴望程度
   - 随时间自然增长（每分钟+1点）
   - 被抚摸时重置为0
   - 影响：高渴望时更容易表现负面情绪

3. **随机兴奋度（Random Excitement）**: 0-100
   - 代表宠物对外界刺激的随机反应
   - 每分钟有30%概率随机变化（±20点）
   - 特殊事件触发时大幅变化
   - 影响：创造不可预测的趣味行为

#### 情感状态决策逻辑

| 当前状态             | 触发条件                                                                 | 持续时间       |
|----------------------|--------------------------------------------------------------------------|---------------|
| **中性 (neutral)**   | 默认状态，当不满足其他条件时                                              | 无限          |
| **开心 (happy)**     | 情感能量 > 70 且 互动渴望 < 30                                           | 15-30秒       |
| **兴奋 (excited)**   | 随机兴奋度 > 80 或 (情感能量 > 60 且 刚被抚摸)                           | 10-20秒       |
| **好奇 (curious)**   | 系统事件触发（如新通知）或 随机兴奋度 > 70                                | 10-15秒       | 
| **困倦 (sleepy)**    | 情感能量 < 30 或 系统空闲时间 > 30分钟                                    | 直到能量恢复   | 
| **失望 (disappointed)** | 互动渴望 > 70 且 2小时内无互动                                           | 20-40秒       | 
| **难过 (sad)**       | 互动渴望 > 80 且 情感能量 < 40                                           | 30-60秒       | 
| **生气 (angry)**     | 被频繁快速点击（3秒内>5次）或 系统事件（如强制关闭）                       | 10-15秒       | 

### 交互设计
**主要交互：抚摸**
   - 鼠标悬停在宠物上：宠物轻微抬头（预互动状态）
   - 鼠标点击并按住：播放抚摸动画，情感能量增加
   - 释放鼠标：显示心形气泡，互动渴望重置

**被动交互**
   - 系统通知时：宠物短暂变为好奇状态
   - 用户长时间不操作：互动渴望增加

**环境响应**：
   - 夜间自动增加困倦概率
   - 系统负载高时表现紧张（可复用生气动画）
**渐进式情感**：
   ```mermaid
   graph LR
   neutral -->|被抚摸| happy
   happy -->|持续互动| excited
   neutral -->|长时间忽视| disappointed
   disappointed -->|继续忽视| sad
   sad -->|持续忽视| sleepy
   ```

## 开发相关

### 开发环境要求
- .NET 9.0 SDK
- Python 3.8+（用于GIF处理工具）

### 构建和发布

#### 日常开发构建
1. 构建项目
```powershell
dotnet build
```

2. 运行应用
```powershell
dotnet run
```

#### 发布新版本
项目提供了一键发布脚本 `scripts/publish.ps1`，可以自动完成版本管理、打包和发布：

1. 正常发布（使用当前版本号）：
```powershell
powershell -ExecutionPolicy ByPass -File ".\scripts\publish.ps1"
```

2. 发布新版本（自动递增版本号）：
```powershell
powershell -ExecutionPolicy ByPass -File ".\scripts\publish.ps1" -IncrementVersion
```

脚本会自动：
- 更新版本号（使用 `-IncrementVersion` 时）
- 同步更新 AssemblyInfo.cs 中的版本信息
- 生成两个版本的发布包：
  - 完整版（约60MB，包含运行时）
  - 框架依赖版（约2MB，需要安装.NET Runtime）
- 创建包含版本信息的 README.txt
- 打包所有必需文件
- 清理临时文件和构建目录

发布包会自动生成在 `Release` 目录下：
- `DesktopPet-vX.X.X-full.zip`：完整版
- `DesktopPet-vX.X.X-framework.zip`：框架依赖版

版本号管理：
- 版本信息存储在 `version.json` 中
- 包含版本号、发布日期、更新说明等
- 可以手动编辑以进行大版本更新

### GIF资源处理
项目包含多个Python脚本用于处理GIF资源：
- `gif_classifier.py`: GIF表情分类工具
- `gif_resizer.py`: 统一GIF尺寸
- `gif_size_analyzer.py`: 分析GIF尺寸分布

使用方法：
```powershell
cd scripts
python gif_resizer.py
```

## 配置说明

### 设置选项
- **动画设置**
  - 默认动画间隔：控制GIF切换频率
  - 特殊状态动画间隔：针对不同情感状态的切换频率
  
- **情感参数**
  - 情感能量衰减率：每分钟情感值降低速度
  - 互动渴望增长率：每分钟互动需求增加速度
  - 随机兴奋概率：触发随机行为的概率

- **时间设置**
  - 夜间模式时间：设置夜间行为模式的时间范围

### 固定显示模式
1. 在右键菜单中选择"固定显示模式"
2. 从左侧列表选择要显示的GIF
3. 设置切换间隔
4. 点击"启用"或"关闭"来控制模式

## 常见问题

### Q: 如何更改宠物大小？
A: 目前所有GIF都统一为150x150像素。如需调整，可使用scripts目录下的gif_resizer.py处理。

### Q: 如何添加新的表情？
A: 将新的GIF文件放入对应情感文件夹（如happy/、sad/等），程序会自动加载。请注意上传的表情应是背景透明的。

### Q: 为什么有时会显示生气表情？
A: 快速连续点击（3秒内超过5次）会触发生气状态，这是一个保护机制。

## 代码规范
项目遵循以下结构：
- Models: 业务逻辑和数据模型
- Views: 用户界面相关代码
- Base: 应用程序基础设施
- scripts: 开发辅助工具（不随应用分发）
