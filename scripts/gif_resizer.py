import os
from PIL import Image
from pathlib import Path
import shutil
import numpy as np

def extract_and_resize_frames(im, size):
    """
    提取并调整GIF帧的大小，同时保持透明度和帧间关系
    """
    frames = []
    
    try:
        while True:
            # 转换为RGBA以保持透明度
            if im.mode == 'P':
                frame = im.convert('RGBA')
            else:
                frame = im.copy().convert('RGBA')
                
            # 调整大小
            if frame.size != size:
                frame = frame.resize(size, Image.Resampling.LANCZOS)
            
            # 将图像转换为numpy数组以便处理
            frame_array = np.array(frame)
            
            # 确保完全透明的像素保持透明
            alpha_mask = frame_array[..., 3] == 0
            frame_array[alpha_mask] = [0, 0, 0, 0]
            
            # 转回Image对象
            frame = Image.fromarray(frame_array)
            frames.append(frame)
            
            # 移动到下一帧
            im.seek(im.tell() + 1)
    except EOFError:
        pass
    
    return frames

def resize_gif(path, size=(150, 150)):
    """
    调整GIF大小，消除闪烁
    """
    with Image.open(path) as im:
        # 如果已经是目标尺寸且只有一帧，直接返回
        if im.size == size and getattr(im, 'n_frames', 1) == 1:
            return

        # 获取原始GIF的信息
        duration = im.info.get('duration', 100)
        loop = im.info.get('loop', 0)
        
        # 提取和调整所有帧
        frames = extract_and_resize_frames(im, size)
        
        if len(frames) > 1:
            # 保存优化后的GIF
            frames[0].save(
                path,
                save_all=True,
                append_images=frames[1:],
                duration=duration,
                loop=loop,
                disposal=2,  # 使用恢复到背景色的方式
                optimize=False,  # 禁用优化以避免产生额外的闪烁
                quality=95
            )
        else:
            # 单帧图片直接保存
            frames[0].save(path, quality=95)

def process_gifs(root_dir, target_size=(150, 150)):
    """
    处理目录下所有GIF文件
    """
    # 创建备份目录
    backup_dir = Path(root_dir).parent / "dog_gifs_backup"
    if not backup_dir.exists():
        shutil.copytree(root_dir, backup_dir)
        print(f"已创建原始GIF备份到: {backup_dir}")
    
    # 遍历所有情感文件夹
    total_processed = 0
    for emotion_dir in Path(root_dir).iterdir():
        if not emotion_dir.is_dir():
            continue
            
        print(f"\n处理 {emotion_dir.name} 文件夹...")
        for gif_file in emotion_dir.glob("*.gif"):
            try:
                print(f"处理 {gif_file.name}...")
                original_size = os.path.getsize(gif_file)
                
                resize_gif(gif_file, target_size)
                
                new_size = os.path.getsize(gif_file)
                size_change = (new_size - original_size) / original_size * 100
                print(f"  - 文件大小变化: {size_change:+.1f}%")
                
                total_processed += 1
            except Exception as e:
                print(f"处理 {gif_file.name} 时出错: {e}")
                
    print(f"\n处理完成！共处理 {total_processed} 个文件")
    print(f"原始文件已备份到: {backup_dir}")

if __name__ == "__main__":
    root_dir = Path(__file__).parent.parent / "dog_gifs"
    process_gifs(root_dir, (150, 150))
