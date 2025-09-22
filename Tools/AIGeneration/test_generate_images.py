#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
即梦AI图片生成测试脚本
用于测试API并批量生成GhostPet游戏素材
"""

import requests
import json
import os
import time
from datetime import datetime

# API配置
API_BASE_URL = "http://localhost:8000"
SESSIONID = "4bdbdb72ceda00f6007b6d249c1c6879"  # 你的即梦AI sessionid

# 输出目录
OUTPUT_DIR = "Assets/GeneratedAssets"

# GhostPet游戏素材提示词
GAME_PROMPTS = {
    "ui_elements": {
        "health_bar_frame": "ancient chinese style health bar frame, wooden texture with gold inlay, jade corners, empty health container, traditional border design, transparent background, game UI asset, high quality, PNG format",
        "health_fill": "red liquid health fill, glowing crimson energy, flowing texture, semi-transparent, ancient elixir style, game UI element, smooth gradient, PNG with alpha channel",
        "score_panel": "traditional chinese scroll panel, aged parchment texture, gold decorative borders, corner tassels, semi-transparent, score display background, game UI, elegant design, PNG format",
        "sword_status_panel": "hexagonal sword holder panel, dark wood with gold trim, six sword slot indicators, ancient chinese design, game UI element, transparent background, PNG format",
        "pause_menu_bg": "large chinese traditional window panel, dark wood frame, semi-transparent paper center, gold corner decorations, pause menu background, game UI, PNG with alpha"
    },
    "characters": {
        "basic_ghost": "chinese ghost spirit, translucent white, glowing eyes, tattered robes, menacing pose, 2D game enemy sprite, pixel art, PNG with alpha",
        "strong_ghost": "powerful ghost demon, dark aura, red eyes, larger size, intimidating presence, 2D game boss sprite, detailed pixel art, PNG format",
        "player_character": "mystical warrior, ancient chinese robes, floating meditation pose, ethereal aura, controlling flying swords, 2D game sprite, pixel art style, transparent background, PNG format"
    },
    "weapons": {
        "flying_sword": "elegant chinese flying sword, silver blade, gold hilt, mystical runes, glowing edge, hovering position, 2D game weapon sprite, high detail, PNG format",
        "sword_trail": "sword afterimage trail, blue energy, motion blur, fading opacity, mystical energy trail, 2D game effect, PNG with alpha channel",
        "hit_effect": "sword strike impact, golden sparks, energy burst, hit effect animation, chinese mystical style, 2D game effects, frame sequence, PNG format"
    }
}

class JimengImageGenerator:
    def __init__(self, sessionid, base_url=API_BASE_URL):
        self.sessionid = sessionid
        self.base_url = base_url
        self.headers = {
            "Authorization": f"Bearer {sessionid}",
            "Content-Type": "application/json"
        }
        
    def test_connection(self):
        """测试API连接"""
        try:
            response = requests.get(f"{self.base_url}/ping")
            print(f"✅ API连接测试: {response.status_code}")
            return response.status_code == 200
        except Exception as e:
            print(f"❌ API连接失败: {e}")
            return False
    
    def generate_image(self, prompt, category="test", name="image", 
                      width=1024, height=1024, model="jimeng-3.0"):
        """生成单张图片"""
        
        data = {
            "model": model,
            "prompt": prompt,
            "negativePrompt": "",
            "width": width,
            "height": height,
            "sample_strength": 0.5
        }
        
        print(f"🎨 正在生成: {category}/{name}")
        print(f"📝 提示词: {prompt[:100]}...")
        
        try:
            response = requests.post(
                f"{self.base_url}/v1/images/generations", 
                headers=self.headers, 
                json=data,
                timeout=60
            )
            
            if response.status_code == 200:
                result = response.json()
                if "data" in result and result["data"]:
                    image_url = result["data"][0]["url"]
                    return self.download_image(image_url, category, name)
                else:
                    print(f"❌ 响应中没有图片数据: {result}")
                    return None
            else:
                print(f"❌ API请求失败: {response.status_code} - {response.text}")
                return None
                
        except Exception as e:
            print(f"❌ 生成图片失败: {e}")
            return None
    
    def download_image(self, url, category, name):
        """下载图片到本地"""
        try:
            # 创建目录
            category_dir = os.path.join(OUTPUT_DIR, category)
            os.makedirs(category_dir, exist_ok=True)
            
            # 下载图片
            response = requests.get(url, timeout=30)
            if response.status_code == 200:
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                filename = f"{name}_{timestamp}.jpg"
                filepath = os.path.join(category_dir, filename)
                
                with open(filepath, 'wb') as f:
                    f.write(response.content)
                
                print(f"✅ 图片已保存: {filepath}")
                return filepath
            else:
                print(f"❌ 下载失败: {response.status_code}")
                return None
                
        except Exception as e:
            print(f"❌ 下载图片失败: {e}")
            return None
    
    def batch_generate(self, prompts_dict, delay=2):
        """批量生成图片"""
        results = {}
        total_count = sum(len(prompts) for prompts in prompts_dict.values())
        current_count = 0
        
        print(f"🚀 开始批量生成，共 {total_count} 张图片...")
        
        for category, prompts in prompts_dict.items():
            print(f"\n📁 正在处理类别: {category}")
            results[category] = {}
            
            for name, prompt in prompts.items():
                current_count += 1
                print(f"\n🔄 进度: {current_count}/{total_count}")
                
                filepath = self.generate_image(prompt, category, name)
                results[category][name] = filepath
                
                if filepath:
                    print(f"✅ 成功生成: {category}/{name}")
                else:
                    print(f"❌ 生成失败: {category}/{name}")
                
                # 延迟避免请求过频
                if current_count < total_count:
                    print(f"⏱️ 等待 {delay} 秒...")
                    time.sleep(delay)
        
        return results

def main():
    print("=" * 50)
    print("🎮 GhostPet游戏素材生成器")
    print("=" * 50)
    
    # 检查sessionid
    if SESSIONID == "YOUR_SESSIONID_HERE":
        print("⚠️ 请先设置你的sessionid:")
        print("1. 访问 https://jimeng.jianying.com/ 并登录")
        print("2. 按F12打开开发者工具")
        print("3. Application > Cookies > 复制sessionid值")
        print("4. 修改脚本中的SESSIONID变量")
        return
    
    # 创建生成器
    generator = JimengImageGenerator(SESSIONID)
    
    # 测试连接
    if not generator.test_connection():
        print("❌ 无法连接到API服务，请确保服务已启动")
        return
    
    print("✅ API服务连接正常")
    
    # 询问用户要执行什么操作
    print("\n请选择操作:")
    print("1. 生成单张测试图片（基础鬼怪敌人）")
    print("2. 批量生成所有游戏素材")
    print("3. 只生成UI元素")
    print("4. 只生成角色素材")
    print("5. 只生成武器素材")
    
    choice = input("\n请输入选择 (1-5): ").strip()
    
    if choice == "1":
        # 测试生成基础鬼怪
        prompt = GAME_PROMPTS["characters"]["basic_ghost"]
        result = generator.generate_image(prompt, "test", "basic_ghost")
        if result:
            print(f"\n🎉 测试成功！图片保存在: {result}")
        else:
            print("\n❌ 测试失败")
    
    elif choice == "2":
        # 批量生成所有素材
        results = generator.batch_generate(GAME_PROMPTS)
        print(f"\n🎉 批量生成完成！结果保存在: {OUTPUT_DIR}")
        
    elif choice == "3":
        # 只生成UI元素
        ui_prompts = {"ui_elements": GAME_PROMPTS["ui_elements"]}
        results = generator.batch_generate(ui_prompts)
        print(f"\n🎉 UI元素生成完成！")
        
    elif choice == "4":
        # 只生成角色
        char_prompts = {"characters": GAME_PROMPTS["characters"]}
        results = generator.batch_generate(char_prompts)
        print(f"\n🎉 角色素材生成完成！")
        
    elif choice == "5":
        # 只生成武器
        weapon_prompts = {"weapons": GAME_PROMPTS["weapons"]}
        results = generator.batch_generate(weapon_prompts)
        print(f"\n🎉 武器素材生成完成！")
    
    else:
        print("❌ 无效选择")

if __name__ == "__main__":
    main()
