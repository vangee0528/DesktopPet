import os
import shutil
import tkinter as tk
from tkinter import ttk
from PIL import Image, ImageTk
from tkinter import messagebox
import json

class GifClassifier:
    def __init__(self, root):
        self.root = root
        self.root.title("GIF 分类工具")
        
        # 设置窗口大小和位置
        window_width = 800
        window_height = 600
        screen_width = root.winfo_screenwidth()
        screen_height = root.winfo_screenheight()
        x = (screen_width - window_width) // 2
        y = (screen_height - window_height) // 2
        self.root.geometry(f"{window_width}x{window_height}+{x}+{y}")

        # 初始化变量
        self.gif_files = []
        self.current_index = 0
        self.classifications = {}
        self.current_image = None
        self.animation = None

        # 创建界面元素
        self.create_widgets()
        
        # 加载GIF文件列表
        self.load_gif_files()
        
        # 加载已有的分类结果
        self.load_classifications()
        
        # 显示第一个GIF
        self.show_current_gif()

    def create_widgets(self):
        # 创建主框架
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))

        # 图片显示区域
        self.image_label = ttk.Label(main_frame)
        self.image_label.grid(row=0, column=0, columnspan=3, pady=10)

        # 文件名标签
        self.filename_label = ttk.Label(main_frame, text="")
        self.filename_label.grid(row=1, column=0, columnspan=3, pady=5)

        # 进度标签
        self.progress_label = ttk.Label(main_frame, text="")
        self.progress_label.grid(row=2, column=0, columnspan=3, pady=5)

        # 按钮区域
        btn_frame = ttk.Frame(main_frame)
        btn_frame.grid(row=3, column=0, columnspan=3, pady=10)

        ttk.Button(btn_frame, text="保留", command=lambda: self.classify_current("keep")).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="删除", command=lambda: self.classify_current("delete")).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="上一个", command=self.show_previous).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="下一个", command=self.show_next).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="保存并退出", command=self.save_and_quit).pack(side=tk.LEFT, padx=5)

        # 绑定键盘事件
        self.root.bind('<Left>', lambda e: self.show_previous())
        self.root.bind('<Right>', lambda e: self.show_next())
        self.root.bind('k', lambda e: self.classify_current("keep"))
        self.root.bind('d', lambda e: self.classify_current("delete"))
        self.root.bind('q', lambda e: self.save_and_quit())

    def load_gif_files(self):
        gif_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs")
        self.gif_files = [f for f in os.listdir(gif_folder) if f.lower().endswith('.gif')]
        self.gif_files.sort()

    def load_classifications(self):
        try:
            with open('classifications.json', 'r', encoding='utf-8') as f:
                self.classifications = json.load(f)
        except FileNotFoundError:
            self.classifications = {}

    def save_classifications(self):
        with open('classifications.json', 'w', encoding='utf-8') as f:
            json.dump(self.classifications, f, indent=4, ensure_ascii=False)

    def show_current_gif(self):
        if not self.gif_files:
            return

        # 更新标签
        current_file = self.gif_files[self.current_index]
        self.filename_label.config(text=current_file)
        self.progress_label.config(text=f"进度: {self.current_index + 1}/{len(self.gif_files)}")

        # 停止当前动画
        if self.animation is not None:
            self.image_label.after_cancel(self.animation)
            self.animation = None

        # 加载新的GIF
        gif_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs", current_file)
        self.animate_gif(gif_path)

        # 显示已有的分类结果（如果有）
        if current_file in self.classifications:
            status = "保留" if self.classifications[current_file] == "keep" else "删除"
            self.filename_label.config(text=f"{current_file} ({status})")

    def animate_gif(self, gif_path):
        try:
            # 使用PIL打开GIF
            gif = Image.open(gif_path)
            frames = []
            
            try:
                while True:
                    # 调整大小，保持比例
                    current_frame = gif.copy()
                    current_frame.thumbnail((300, 300), Image.Resampling.LANCZOS)
                    frames.append(ImageTk.PhotoImage(current_frame))
                    gif.seek(gif.tell() + 1)
            except EOFError:
                pass

            # 显示第一帧
            if frames:
                self.show_frame(frames, 0, gif.info.get('duration', 100))

        except Exception as e:
            print(f"Error loading GIF: {e}")
            self.image_label.config(text="Error loading GIF")

    def show_frame(self, frames, frame_index, duration):
        if not frames:
            return

        frame = frames[frame_index]
        self.image_label.config(image=frame)
        self.current_image = frame  # 保持引用
        
        # 计算下一帧的索引
        next_frame = (frame_index + 1) % len(frames)
        
        # 设置显示下一帧的定时器
        self.animation = self.root.after(duration, lambda: self.show_frame(frames, next_frame, duration))

    def classify_current(self, classification):
        if not self.gif_files:
            return

        current_file = self.gif_files[self.current_index]
        self.classifications[current_file] = classification
        self.show_next()

    def show_next(self):
        if self.current_index < len(self.gif_files) - 1:
            self.current_index += 1
            self.show_current_gif()

    def show_previous(self):
        if self.current_index > 0:
            self.current_index -= 1
            self.show_current_gif()

    def save_and_quit(self):
        if messagebox.askyesno("确认", "是否保存分类结果并退出？"):
            self.save_classifications()
            # 应用分类结果
            self.apply_classifications()
            self.root.quit()

    def apply_classifications(self):
        gif_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs")
        keep_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs_transparent")
        delete_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs_nontransparent")

        # 创建目标文件夹
        os.makedirs(keep_folder, exist_ok=True)
        os.makedirs(delete_folder, exist_ok=True)

        # 移动文件
        for filename, classification in self.classifications.items():
            source_path = os.path.join(gif_folder, filename)
            if classification == "keep":
                dest_path = os.path.join(keep_folder, filename)
            else:
                dest_path = os.path.join(delete_folder, filename)
            
            if os.path.exists(source_path):
                shutil.copy2(source_path, dest_path)

if __name__ == "__main__":
    root = tk.Tk()
    app = GifClassifier(root)
    root.mainloop()
