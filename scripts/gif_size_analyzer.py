import os
from PIL import Image
from collections import defaultdict
import matplotlib.pyplot as plt
from pathlib import Path

def analyze_gif_sizes(root_dir):
    # 存储所有GIF的尺寸信息
    sizes = []
    size_frequency = defaultdict(int)
    
    # 遍历所有情感文件夹
    for emotion_dir in Path(root_dir).iterdir():
        if emotion_dir.is_dir():
            print(f"\n分析 {emotion_dir.name} 文件夹...")
            
            # 遍历该情感文件夹中的所有GIF
            for gif_file in emotion_dir.glob("*.gif"):
                try:
                    with Image.open(gif_file) as img:
                        width, height = img.size
                        sizes.append((width, height))
                        size_frequency[(width, height)] += 1
                        print(f"{gif_file.name}: {width}x{height}")
                except Exception as e:
                    print(f"处理 {gif_file} 时出错: {e}")

    return sizes, size_frequency

def print_statistics(size_frequency):
    print("\n尺寸分布统计:")
    print("=" * 40)
    
    # 按频率排序
    sorted_sizes = sorted(size_frequency.items(), key=lambda x: x[1], reverse=True)
    
    for (width, height), count in sorted_sizes:
        print(f"{width}x{height}: {count}个文件")

def plot_size_distribution(sizes):
    widths, heights = zip(*sizes)
    
    plt.figure(figsize=(10, 6))
    plt.scatter(widths, heights, alpha=0.5)
    plt.title("GIF尺寸分布")
    plt.xlabel("宽度 (像素)")
    plt.ylabel("高度 (像素)")
    plt.grid(True)
    
    # 添加尺寸标注
    for size, count in size_frequency.items():
        plt.annotate(f"({size[0]}, {size[1]})\n{count}个",
                    xy=size,
                    xytext=(5, 5),
                    textcoords='offset points')
    
    plt.savefig(os.path.join(os.path.dirname(root_dir), "gif_size_distribution.png"))
    print("\n尺寸分布图已保存为 gif_size_distribution.png")

if __name__ == "__main__":
    root_dir = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs")
    sizes, size_frequency = analyze_gif_sizes(root_dir)
    print_statistics(size_frequency)
    plot_size_distribution(sizes)
