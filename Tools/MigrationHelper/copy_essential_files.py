#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
GhostPet 3D迁移助手 - 自动复制核心文件
帮助快速将2D项目核心文件迁移到3D项目
"""

import os
import shutil
import argparse
from pathlib import Path

# 需要完整复制的文件夹
COPY_FOLDERS = [
    "Assets/GeneratedAssets",
    "Assets/Scripts/Core", 
    "Assets/Scripts/UI",
    "Assets/Scripts/Input",
    "Assets/Scripts/Health",
    "Tools",
]

# 需要复制的单个文件
COPY_FILES = [
    "Assets/InputSystem_Actions.inputactions",
    "ProjectSettings/InputManager.asset",
    "ProjectSettings/TagManager.asset",
]

# 3D适配脚本模板（需要用户手动替换）
TEMPLATE_SCRIPTS = {
    "Assets/Scripts/Player/PlayerController.cs": "docs/3D适配脚本模板/Player3DController.cs",
    "Assets/Scripts/Camera/CameraFollow.cs": "docs/3D适配脚本模板/ThirdPersonCamera.cs", 
    "Assets/Scripts/Enemy/EnemyAI.cs": "docs/3D适配脚本模板/Enemy3DAI.cs",
}

def copy_folder(src_path, dest_path):
    """复制文件夹"""
    try:
        if os.path.exists(dest_path):
            shutil.rmtree(dest_path)
        shutil.copytree(src_path, dest_path)
        print(f"✅ 复制文件夹: {src_path} → {dest_path}")
        return True
    except Exception as e:
        print(f"❌ 复制文件夹失败: {src_path} - {e}")
        return False

def copy_file(src_path, dest_path):
    """复制单个文件"""
    try:
        # 确保目标目录存在
        os.makedirs(os.path.dirname(dest_path), exist_ok=True)
        shutil.copy2(src_path, dest_path)
        print(f"✅ 复制文件: {src_path} → {dest_path}")
        return True
    except Exception as e:
        print(f"❌ 复制文件失败: {src_path} - {e}")
        return False

def main():
    parser = argparse.ArgumentParser(description="GhostPet 3D迁移助手")
    parser.add_argument("source_project", help="源项目路径（2D项目）")
    parser.add_argument("target_project", help="目标项目路径（3D项目）")
    parser.add_argument("--dry-run", action="store_true", help="仅显示将要执行的操作，不实际复制")
    
    args = parser.parse_args()
    
    source_root = Path(args.source_project)
    target_root = Path(args.target_project)
    
    print("🚀 GhostPet 3D迁移助手")
    print("=" * 50)
    print(f"📂 源项目: {source_root}")
    print(f"📂 目标项目: {target_root}")
    print(f"🔧 模式: {'预览模式' if args.dry_run else '执行模式'}")
    print()
    
    if not source_root.exists():
        print(f"❌ 错误: 源项目路径不存在: {source_root}")
        return
        
    if not target_root.exists():
        print(f"❌ 错误: 目标项目路径不存在: {target_root}")
        return
    
    success_count = 0
    total_operations = len(COPY_FOLDERS) + len(COPY_FILES)
    
    print("📁 准备复制的文件夹:")
    for folder in COPY_FOLDERS:
        src_path = source_root / folder
        dest_path = target_root / folder
        
        if src_path.exists():
            print(f"   ✓ {folder}")
            if not args.dry_run:
                if copy_folder(str(src_path), str(dest_path)):
                    success_count += 1
            else:
                success_count += 1
        else:
            print(f"   ⚠️  {folder} (源路径不存在)")
    
    print()
    print("📄 准备复制的文件:")
    for file_path in COPY_FILES:
        src_path = source_root / file_path
        dest_path = target_root / file_path
        
        if src_path.exists():
            print(f"   ✓ {file_path}")
            if not args.dry_run:
                if copy_file(str(src_path), str(dest_path)):
                    success_count += 1
            else:
                success_count += 1
        else:
            print(f"   ⚠️  {file_path} (源文件不存在)")
    
    print()
    print("🔄 需要手动替换的3D适配脚本:")
    for original, template in TEMPLATE_SCRIPTS.items():
        template_path = source_root / template
        target_script_path = target_root / original
        
        if template_path.exists():
            print(f"   📋 {original}")
            print(f"      使用模板: {template}")
            
            if not args.dry_run:
                # 创建目标目录
                os.makedirs(os.path.dirname(target_script_path), exist_ok=True)
                # 复制模板文件
                if copy_file(str(template_path), str(target_script_path)):
                    print(f"      ✅ 已复制模板到目标位置")
                else:
                    print(f"      ❌ 模板复制失败")
        else:
            print(f"   ⚠️  模板文件不存在: {template}")
    
    print()
    print("=" * 50)
    
    if args.dry_run:
        print("🔍 预览完成！使用 --dry-run=false 执行实际复制")
    else:
        print(f"🎯 迁移完成！成功: {success_count}/{total_operations}")
        print()
        print("📋 后续手动步骤:")
        print("1. 在Unity中刷新Assets")
        print("2. 检查并修复任何编译错误")  
        print("3. 按照'快速迁移检查清单.md'完成剩余设置")
        print("4. 创建测试场景并验证功能")
        print()
        print("🎮 预期结果: 可运行的3D版本GhostPet游戏！")

if __name__ == "__main__":
    main()
