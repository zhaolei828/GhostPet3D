#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
GhostPet 游戏素材生成工具 - 支持参数化生成
可以选择性生成指定的素材或类别，支持批量生成
"""

import requests
import json
import os
import time
import argparse
from datetime import datetime

# API配置
API_BASE_URL = "http://localhost:8000"
SESSIONID = "4bdbdb72ceda00f6007b6d249c1c6879"

# 基础输出目录
BASE_OUTPUT_DIR = "Assets/GeneratedAssets"

# 规范的文件夹结构
FOLDERS = {
    "characters": f"{BASE_OUTPUT_DIR}/characters",
    "weapons": f"{BASE_OUTPUT_DIR}/weapons", 
    "ui_elements": f"{BASE_OUTPUT_DIR}/ui_elements",
    "effects": f"{BASE_OUTPUT_DIR}/effects"
}

# 完整的游戏素材提示词 - 优化版
GAME_ASSETS = {
    "characters": {
        "player_character": {
            "prompt": "mystical warrior cultivator, ancient chinese robes, commanding pose with outstretched arms, channeling magical energy, spellcasting gestures, ethereal aura swirling around body, concentrating on mystical energy control, meditation pose, no weapons, no swords, clean character only, 2D game character sprite, transparent background, isolated, PNG format, game asset",
            "filename": "player_character.png"
        },
        "basic_ghost": {
            "prompt": "chinese ghost spirit, translucent white appearance, glowing red eyes, tattered traditional robes, menacing floating pose, 2D game enemy sprite, white background, isolated character, PNG format",
            "filename": "basic_ghost.png"
        },
        "strong_ghost": {
            "prompt": "powerful ghost demon, dark smoky aura, fierce red glowing eyes, larger intimidating size, torn black robes, boss enemy sprite, white background, isolated, PNG format, 2D game asset, image background removed",
            "filename": "strong_ghost.png"
        }
    },
    "weapons": {
        "flying_sword": {
            "prompt": "fantasy crystal sword, translucent silver-white blade with sharp geometric design, ornate hilt with square blue gemstone decorations, modern game weapon style, floating horizontally, clean magical weapon, no energy effects, transparent background, isolated weapon, PNG format, 2D game sprite",
            "filename": "flying_sword.png"
        },
        "sword_energy": {
            "prompt": "fantasy crystal sword, translucent silver-white blade with sharp geometric design, ornate hilt with square blue gemstone decorations, modern game weapon style, floating horizontally, PLUS intense blue magical energy aura swirling around entire sword, flowing energy wisps, power charging effect, mystical flames, magical enhancement glow, transparent background, isolated weapon, PNG format, 2D game sprite",
            "filename": "sword_energy.png"
        }
    },
    "effects": {
        "sword_trail": {
            "prompt": "sword afterimage trail, blue energy motion blur, fading opacity effect, mystical sword path, white background, isolated effect, PNG format, 2D game trail effect",
            "filename": "sword_trail.png"
        },
        "hit_effect": {
            "prompt": "sword strike impact sparks, golden energy burst, hit collision effect, impact particles, white background, isolated effect, PNG format, 2D game impact animation",
            "filename": "hit_effect.png"
        },
        "enemy_death": {
            "prompt": "ghost dissipation effect, white smoke particles, spirit fading away, death animation effect, white background, isolated effect, PNG format, enemy destruction effect",
            "filename": "enemy_death.png"
        }
    },
    "ui_elements": {
        "health_bar_frame": {
            "prompt": "ancient chinese health bar frame, wooden texture with gold inlay, jade corner decorations, traditional border design, white background, isolated UI element, PNG format",
            "filename": "health_bar_frame.png"
        },
        "score_panel": {
            "prompt": "traditional chinese scroll panel, aged parchment texture, gold decorative borders, corner tassels, score display background, white background, isolated UI, PNG format",
            "filename": "score_panel.png"
        },
        "sword_icon": {
            "prompt": "small sword status icon, minimalist chinese sword symbol, golden color, simple design for UI, white background, isolated icon, PNG format, 64x64 size",
            "filename": "sword_icon.png"
        }
    }
}

class AssetGenerator:
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            "Authorization": f"Bearer {SESSIONID}",
            "Content-Type": "application/json",
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        })
    
    def create_folders(self):
        """创建文件夹结构"""
        for folder_name, folder_path in FOLDERS.items():
            os.makedirs(folder_path, exist_ok=True)
            print(f"📁 创建文件夹: {folder_name}")
    
    def generate_image(self, prompt, category, filename):
        """生成单张图片"""
        url = f"{API_BASE_URL}/v1/images/generations"
        
        payload = {
            "model": "jimeng",
            "prompt": prompt,
            "sessionid": SESSIONID,
            "size": "1024x1024",
            "style": "realistic",
            "quality": "standard"
        }
        
        print(f"🎨 正在生成: {category}/{filename}")
        print(f"📝 提示词: {prompt[:80]}...")
        
        try:
            response = self.session.post(url, json=payload, timeout=60)
            response.raise_for_status()
            
            result = response.json()
            
            if 'data' in result and result['data'] and len(result['data']) > 0:
                image_url = result['data'][0]['url']
                return self.download_image(image_url, category, filename)
            else:
                print(f"❌ 响应中没有图片数据: {result}")
                return False
                
        except requests.exceptions.RequestException as e:
            print(f"❌ API请求失败: {e}")
            return False
        except json.JSONDecodeError as e:
            print(f"❌ 响应解析失败: {e}")
            return False
    
    def download_image(self, url, category, filename):
        """下载图片到指定文件夹"""
        try:
            print(f"📥 正在下载...")
            response = requests.get(url, timeout=30)
            response.raise_for_status()
            
            filepath = os.path.join(FOLDERS[category], filename)
            with open(filepath, 'wb') as f:
                f.write(response.content)
            
            print(f"✅ 保存成功: {category}/{filename}")
            return True
            
        except Exception as e:
            print(f"❌ 下载失败: {e}")
            return False

def parse_arguments():
    """解析命令行参数"""
    parser = argparse.ArgumentParser(
        description="GhostPet 游戏素材生成工具",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
使用示例:
  python regenerate_all_assets.py                    # 生成所有素材
  python regenerate_all_assets.py --list             # 显示所有可用素材
  python regenerate_all_assets.py --category characters  # 只生成角色类素材
  python regenerate_all_assets.py --asset player_character  # 只生成玩家角色
  python regenerate_all_assets.py --asset player_character basic_ghost  # 生成多个指定素材
        """
    )
    
    parser.add_argument(
        '--category', '-c',
        choices=['characters', 'weapons', 'effects', 'ui_elements'],
        help='指定要生成的素材类别'
    )
    
    parser.add_argument(
        '--asset', '-a',
        nargs='+',
        help='指定要生成的具体素材名称（可指定多个）'
    )
    
    parser.add_argument(
        '--list', '-l',
        action='store_true',
        help='显示所有可用的素材列表'
    )
    
    return parser.parse_args()

def list_all_assets():
    """显示所有可用的素材"""
    print("🎮 GhostPet 可用素材列表")
    print("=" * 50)
    
    for category_name, assets in GAME_ASSETS.items():
        print(f"\n📁 {category_name}:")
        for asset_name in assets.keys():
            print(f"   ├── {asset_name}")
    
    print(f"\n💡 使用方法:")
    print(f"   --category {list(GAME_ASSETS.keys())[0]}  # 生成整个类别")
    print(f"   --asset {list(list(GAME_ASSETS.values())[0].keys())[0]}     # 生成单个素材")

def get_assets_to_generate(args):
    """根据参数确定要生成的素材"""
    if args.asset:
        # 生成指定的素材
        assets_to_generate = []
        for asset_name in args.asset:
            found = False
            for category_name, assets in GAME_ASSETS.items():
                if asset_name in assets:
                    assets_to_generate.append((category_name, asset_name, assets[asset_name]))
                    found = True
                    break
            if not found:
                print(f"⚠️ 警告: 未找到素材 '{asset_name}'")
        return assets_to_generate
    
    elif args.category:
        # 生成指定类别的所有素材
        if args.category in GAME_ASSETS:
            assets_to_generate = []
            for asset_name, asset_data in GAME_ASSETS[args.category].items():
                assets_to_generate.append((args.category, asset_name, asset_data))
            return assets_to_generate
        else:
            print(f"❌ 错误: 未找到类别 '{args.category}'")
            return []
    
    else:
        # 生成所有素材
        assets_to_generate = []
        for category_name, assets in GAME_ASSETS.items():
            for asset_name, asset_data in assets.items():
                assets_to_generate.append((category_name, asset_name, asset_data))
        return assets_to_generate

def main():
    # 解析命令行参数
    args = parse_arguments()
    
    # 如果用户要求显示素材列表，显示后退出
    if args.list:
        list_all_assets()
        return
    
    # 确定要生成的素材
    assets_to_generate = get_assets_to_generate(args)
    
    if not assets_to_generate:
        print("❌ 没有找到要生成的素材")
        return
    
    generator = AssetGenerator()
    
    print("🎮 GhostPet 游戏素材生成")
    print("=" * 60)
    
    if args.asset:
        print(f"🎯 模式: 生成指定素材 ({len(args.asset)} 个)")
        print(f"📋 素材列表: {', '.join(args.asset)}")
    elif args.category:
        print(f"🎯 模式: 生成类别素材 ({args.category})")
    else:
        print(f"🎯 模式: 生成所有素材")
    
    # 创建文件夹结构
    print("\n📁 创建文件夹结构...")
    generator.create_folders()
    
    total_assets = len(assets_to_generate)
    success_count = 0
    
    # 生成选定的素材
    for i, (category_name, asset_name, asset_data) in enumerate(assets_to_generate, 1):
        print(f"\n📍 [{i}/{total_assets}] {category_name}/{asset_name}")
        
        if generator.generate_image(
            asset_data["prompt"], 
            category_name, 
            asset_data["filename"]
        ):
            success_count += 1
            print(f"✅ 成功")
        else:
            print(f"❌ 失败")
        
        # 添加延迟避免API限制
        if i < total_assets:
            print("⏳ 等待3秒...")
            time.sleep(3)
    
    print("\n" + "=" * 60)
    print(f"🎯 生成完成！成功: {success_count}/{total_assets}")
    
    # 显示已生成的素材
    if success_count > 0:
        print(f"\n✅ 已生成的素材:")
        generated_categories = set()
        for category_name, asset_name, _ in assets_to_generate:
            generated_categories.add(category_name)
            print(f"   ├── {category_name}/{asset_name}")
        
        print(f"\n📂 涉及的文件夹:")
        for category in generated_categories:
            folder_path = FOLDERS[category]
            print(f"   ├── {folder_path}")
    
    print(f"\n🎮 现在可以在Unity中刷新资源了！")

if __name__ == "__main__":
    main()
