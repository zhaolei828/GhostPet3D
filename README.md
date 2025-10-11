# GhostPet3D - 3D幽灵宠物战斗游戏

**从2D到3D的完整迁移项目**  
**原项目**: [GhostPet (2D版本)](https://github.com/zhaolei828/GhostPet.git)

---

## 🎮 游戏简介

GhostPet3D是基于原始2D版本GhostPet的完整3D重制版本。游戏采用Unity 3D引擎，实现了完整的3D战斗系统，包括智能敌人AI、飞剑环绕攻击、NavMesh寻路等现代游戏机制。

### ✨ 核心特性

- 🕹️ **3D角色控制**: WASD移动 + 空格跳跃的流畅3D操作
- ⚔️ **飞剑战斗系统**: 6把飞剑环绕玩家自动攻击敌人
- 🤖 **智能敌人AI**: 基于NavMesh的智能寻路和追击系统
- 📹 **第三人称摄像机**: 平滑跟随和动态视角调整
- 🌍 **3D物理系统**: 完整的重力、碰撞和地面检测
- ⚡ **实时战斗**: 双向攻击系统（玩家飞剑 ↔ 敌人近战）

---

## 🚀 快速开始

### 系统要求

- **Unity版本**: Unity 2023.3 LTS 或更高
- **平台**: Windows, macOS, Linux
- **内存**: 最低4GB RAM
- **存储**: 2GB可用空间

### 安装步骤

1. **克隆项目**
   ```bash
   git clone [GitHub链接]
   cd GhostPet3D
   ```

2. **打开Unity项目**
   - 启动Unity Hub
   - 选择 "Open" → 浏览到 `GhostPet3D` 文件夹
   - 选择项目并打开

3. **运行游戏**
   - 打开场景: `Assets/Scenes/Main3D.unity`
   - 点击播放按钮 ▶️
   - 使用WASD移动，空格跳跃

---

## 🎯 游戏玩法

### 基础操作
- **WASD**: 角色移动
- **空格**: 跳跃
- **鼠标**: 视角控制（跟随摄像机）

### 战斗系统
- **自动攻击**: 飞剑自动攻击范围内的敌人（20单位）
- **能量系统**: 飞剑消耗能量，自动恢复
- **环绕防御**: 6把飞剑持续环绕玩家旋转

### 敌人AI
- **智能寻路**: 敌人使用NavMesh自动寻找最优路径
- **动态追击**: 检测玩家并智能追击（10单位范围）
- **近战攻击**: 接近玩家时进行近距离攻击

---

## 🏗️ 项目架构

### 核心脚本结构

```
Assets/Scripts/
├── Player/
│   └── Player3DController.cs       # 3D玩家控制器
├── Camera/
│   └── ThirdPersonCamera.cs        # 第三人称摄像机
├── Combat/
│   ├── FlyingSword3D.cs           # 3D飞剑逻辑
│   └── FlyingSwordManager3D.cs    # 飞剑管理系统
├── Enemy/
│   ├── Enemy3DAI.cs                # 智能敌人AI
│   └── Enemy3DAI_Simple.cs        # 简化版敌人AI
├── Health/
│   └── HealthSystem.cs            # 血量管理系统
├── Input/
│   └── TouchInputManager.cs       # 输入管理
├── Core/
│   ├── GameManager.cs             # 游戏管理器
│   ├── PoolManager.cs             # 对象池管理
│   └── RuntimeAssetUpdater.cs     # 运行时资源更新
└── UI/
    └── [多个UI脚本]               # UI系统组件
```

### 关键系统

1. **Player3DController**
   - 3D移动和跳跃逻辑
   - 物理交互和碰撞检测
   - 输入处理和响应

2. **FlyingSwordManager3D**
   - 6把飞剑的环绕轨道
   - 自动目标检测和攻击
   - 能量系统管理

3. **Enemy3DAI**
   - NavMesh寻路算法
   - 玩家检测和追击
   - 攻击范围判断

4. **ThirdPersonCamera**
   - 平滑跟随算法
   - 动态距离调整
   - 碰撞避免

---

## 🔧 技术特性

### Unity包依赖

```json
{
  "com.unity.ai.navigation": "2.0.9",           // NavMesh寻路
  "com.unity.inputsystem": "1.14.2",           // 新输入系统
  "com.unity.render-pipelines.universal": "17.2.0", // URP渲染
  "com.unity.ugui": "2.0.0",                   // UI系统
  "com.coplaydev.unity-mcp": "latest"          // Unity MCP自动化
}
```

### 核心技术实现

- **NavMesh Surface**: 使用Unity最新的NavMesh系统替代已弃用的Navigation Static
- **Unity Input System**: 现代化的输入处理框架
- **Universal Render Pipeline**: 高性能渲染管线
- **单例模式**: 核心管理器使用安全的单例实现
- **对象池**: 优化性能的飞剑和特效复用

---

## 🐛 已知问题和解决方案

### 已修复的重大问题

1. **Player对象消失问题**
   - **原因**: FlyingSwordManager3D单例冲突导致Player被销毁
   - **解决**: 修改单例模式，只销毁重复组件而非整个GameObject

2. **NavMesh配置问题**
   - **原因**: 使用已弃用的Navigation Static
   - **解决**: 迁移到NavMesh Surface组件

3. **编译错误**
   - **原因**: 重复类定义和缺失引用
   - **解决**: 清理重复脚本，更新命名空间

### 性能优化

- **对象池管理**: 飞剑和特效使用对象池避免频繁创建销毁
- **LOD系统**: 敌人在远距离时降低更新频率
- **碰撞优化**: 使用Trigger代替复杂碰撞检测

---

## 📦 构建和部署

### 开发构建

1. 打开 `File > Build Settings`
2. 选择目标平台 (PC, Mac, Linux)
3. 点击 `Build And Run`

### 发布构建

1. 在 `Build Settings` 中取消勾选 `Development Build`
2. 启用 `Script Debugging` (可选)
3. 选择优化设置并构建

---

## 🤝 贡献指南

### 开发环境设置

1. 安装Unity 2023.3 LTS
2. 克隆项目到本地
3. 安装Unity MCP扩展（可选，用于自动化）

### 代码风格

- 使用C# 9.0+语法特性
- 遵循Unity命名约定
- 添加详细的XML文档注释
- 使用[Header]和[SerializeField]特性

### 提交规范

```
feat: 添加新功能
fix: 修复bug
docs: 更新文档
refactor: 代码重构
perf: 性能优化
test: 测试相关
```

---

## 📊 项目统计

### 迁移进度

- ✅ **核心系统**: 100% 完成
- ✅ **3D适配**: 100% 完成
- ✅ **AI系统**: 100% 完成
- ✅ **战斗系统**: 100% 完成
- ✅ **UI系统**: 100% 完成（包含游戏结束UI）
- ✅ **特效系统**: 100% 完成（脚印、雪地、溶解特效）
- ✅ **游戏流程**: 100% 完成（开始、游戏中、结束、重启）

### 代码量统计

- **总脚本文件**: 77+
- **总代码行数**: 12000+
- **核心系统文件**: 18
- **UI组件文件**: 23+
- **敌人AI类型**: 2种（近战+幽灵）

---

## 📚 相关资源

### 文档
- [3D迁移状态报告](docs/3D迁移当前状态.md)
- [第5阶段完成报告](docs/第5阶段完成报告.md)
- [NavMesh配置指南](docs/第6阶段_最新NavMesh配置指南.md)

### 外部资源
- [原始2D项目](https://github.com/zhaolei828/GhostPet.git)
- [Unity官方文档](https://docs.unity3d.com/)
- [NavMesh官方指南](https://docs.unity3d.com/Manual/nav-NavigationSystem.html)

---

## 📝 版本历史

### v1.0.0 (当前版本 - 生产就绪)
- ✅ 完整的2D到3D迁移
- ✅ 所有核心系统正常运行
- ✅ NavMesh智能敌人AI（2种敌人类型）
- ✅ 飞剑环绕战斗系统（6把攻击+9把环绕）
- ✅ 第三人称摄像机
- ✅ 完整游戏流程（游戏结束、重启、退出）
- ✅ 敌人生成器（对象池、波次生成、动态难度）
- ✅ UI系统（血量条、分数、飞剑状态、游戏结束界面）
- ✅ 特效系统（脚印、雪地环境、溶解效果）

### 未来计划
- ⭐ 更多敌人类型和AI行为变种
- ⭐ Boss战设计
- ⭐ 关卡系统和progression
- ⭐ 角色升级和技能树
- ⭐ 音效和背景音乐系统

---

## 📄 许可证

本项目基于原始GhostPet项目进行3D适配和扩展。

---

## 💬 联系方式

- **项目维护者**: [原作者信息]
- **3D迁移**: Claude & Unity MCP自动化
- **问题反馈**: 请在GitHub Issues中提交

---

**🎮 享受3D幽灵宠物的战斗体验！**

*最后更新: 2025年10月9日*  
*版本: v1.0.0 - 生产就绪版本*
