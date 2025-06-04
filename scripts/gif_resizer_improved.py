import os
from PIL import Image
from pathlib import Path
import shutil

def resize_gif_frame(frame, size):
    """
    正确处理单个GIF帧的调整
    """
    # 保存原始调色板和透明信息
    palette = frame.getpalette()
    transparency = frame.info.get('transparency', None)
    
    # 如果是P模式（调色板模式），先转换为RGBA
    if frame.mode == 'P':
        frame = frame.convert('RGBA')
    
    # 调整大小，使用LANCZOS采样
    resized = frame.resize(size, Image.Resampling.LANCZOS)
    
    # 转换回调色板模式
    if palette:
        resized = resized.convert('P', palette=Image.Palette.ADAPTIVE, colors=256)
        resized.putpalette(palette)
        
        # 恢复透明信息
        if transparency is not None:
            resized.info['transparency'] = transparency
    
    return resized

def resize_gif(path, size=(150, 150)):
    """
    调整GIF大小，保持动画效果和质量
    """
    with Image.open(path) as im:
        # 如果已经是目标尺寸，直接返回
        if im.size == size:
            return

        # 获取原始GIF的关键信息
        duration = im.info.get('duration', 100)  # 帧持续时间
        loop = im.info.get('loop', 0)           # 循环次数
        disposal = im.info.get('disposal', 2)    # 帧处理方法
        
        frames = []
        try:
            while True:
                # 复制当前帧并调整大小
                current = im.copy()
                resized_frame = resize_gif_frame(current, size)
                
                # 保存帧的处理方法
                resized_frame.info['disposal'] = disposal
                
                frames.append(resized_frame)
                im.seek(im.tell() + 1)
        except EOFError:
            pass

        # 保存调整后的GIF，使用更高质量的设置
        frames[0].save(
            path,
            save_all=True,
            append_images=frames[1:],
            duration=duration,
            loop=loop,
            disposal=disposal,
            optimize=False,  # 禁用优化以保持帧质量
            quality=95      # 使用更高的质量设置
        )

def process_gifs(root_dir, target_size=(150, 150)):
    """
    处理目录下所有GIF文件
    """
    # 创建备份目录
    backup_dir = Path(root_dir).parent / "dog_gifs_backup"
    if not backup_dir.exists():
        # 如果备份目录不存在，创建完整备份
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
                # 获取原始文件大小
                original_size = os.path.getsize(gif_file)
                
                print(f"调整 {gif_file.name} 大小...")
                resize_gif(gif_file, target_size)
                
                # 获取处理后文件大小
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
