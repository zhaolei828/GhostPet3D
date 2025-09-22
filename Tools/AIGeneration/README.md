# AI素材生成工具

## 📖 概述

本目录包含用于GhostPet项目的AI素材生成脚本，使用即梦AI生成游戏所需的各类素材。

## 📁 文件说明

### 生成脚本
- `regenerate_all_assets.py` - **主要脚本**，重新生成所有游戏素材
- `generate_transparent_sword.py` - 专门生成透明背景飞剑的脚本  
- `test_generate_images.py` - 早期测试脚本，包含完整的提示词库

### 配置说明
所有脚本共享以下配置：
```python
API_BASE_URL = "http://localhost:8000"  # 即梦AI本地API地址
SESSIONID = "your_session_id_here"       # 需要从即梦AI网站获取
OUTPUT_DIR = "Assets/GeneratedAssets"    # 输出到Unity资源目录
```

## 🚀 使用方法

### 1. 环境准备
确保即梦AI本地服务正在运行：
```bash
# 在jimeng-free-api目录下
npm start
```

### 2. 配置SESSIONID
在脚本中设置你的即梦AI sessionid（参考docs/即梦AI-SESSIONID获取教程.md）

### 3. 运行生成脚本
```bash
# 从项目根目录运行
python Tools/AIGeneration/regenerate_all_assets.py
```

## 📂 输出结构

生成的素材将按以下结构保存：
```
Assets/GeneratedAssets/
├── characters/          # 角色素材
│   ├── player_character.png
│   ├── basic_ghost.png
│   └── strong_ghost.png
├── weapons/             # 武器素材
│   ├── flying_sword.png
│   └── sword_energy.png
├── effects/             # 特效素材
│   ├── sword_trail.png
│   ├── hit_effect.png
│   └── enemy_death.png
└── ui_elements/         # UI素材
    ├── health_bar_frame.png
    ├── score_panel.png
    └── sword_icon.png
```

## 🎨 提示词优化

所有脚本都包含针对游戏素材优化的提示词：
- 强调白色/透明背景
- 中式古风元素
- 游戏资源风格
- PNG格式输出
- 隔离对象设计

## 🔧 自定义扩展

### 添加新素材类型
1. 在`GAME_ASSETS`字典中添加新分类
2. 在`FOLDERS`字典中添加对应文件夹
3. 设计适合的提示词

### 批量处理
使用`regenerate_all_assets.py`可以一次性重新生成所有素材，适合：
- 更新所有素材风格
- 修改提示词后批量重生成
- 项目初始化

## ⚠️ 注意事项

1. **API限制**: 脚本间有3秒延迟避免API限制
2. **网络连接**: 确保网络连接稳定，下载大图片需要时间
3. **磁盘空间**: 每个PNG文件约400-600KB，确保有足够空间
4. **sessionid有效期**: 定期检查和更新sessionid

## 📚 相关文档

- [AI素材生成指南.md](../../docs/AI素材生成指南.md) - 完整工作流程
- [即梦AI使用说明.md](../../docs/即梦AI使用说明.md) - 详细使用指南
- [即梦AI-SESSIONID获取教程.md](../../docs/即梦AI-SESSIONID获取教程.md) - SESSIONID获取教程
