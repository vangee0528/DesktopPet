import os
import shutil
import tkinter as tk
from tkinter import ttk
from PIL import Image, ImageTk
import json

EMOTIONS = {
    'happy': '开心',
    'sad': '难过',
    'angry': '生气',
    'disappointed': '失望',
    'neutral': '无明显情绪',
    'excited': '兴奋',
    'sleepy': '困倦',
    'curious': '好奇'
}

class EmotionClassifier:
    def __init__(self, root):
        self.root = root
        self.root.title("GIF 情感分类工具")
        
        # 设置窗口大小和位置
        window_width = 1000
        window_height = 700
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
        self.last_emotion = None  # 记住上一次选择的情感

        # 加载已有的分类结果
        self.load_classifications()
        
        # 创建界面
        self.create_widgets()
        
        # 加载GIF文件
        self.load_gif_files()
        
        # 显示第一个GIF
        if self.gif_files:
            self.show_current_gif()

    def create_widgets(self):
        # 创建主框架
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # 左侧：显示区域
        left_frame = ttk.Frame(main_frame)
        left_frame.grid(row=0, column=0, padx=10, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # GIF预览
        self.image_label = ttk.Label(left_frame)
        self.image_label.pack(pady=10)
        
        # 文件信息
        info_frame = ttk.Frame(left_frame)
        info_frame.pack(fill=tk.X, pady=5)
        
        self.filename_label = ttk.Label(info_frame, text="")
        self.filename_label.pack(side=tk.LEFT, padx=5)
        
        self.progress_label = ttk.Label(info_frame, text="")
        self.progress_label.pack(side=tk.RIGHT, padx=5)
        
        # 右侧：情感按钮
        right_frame = ttk.Frame(main_frame)
        right_frame.grid(row=0, column=1, padx=10, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        ttk.Label(right_frame, text="选择情感分类：", font=('Arial', 12)).pack(pady=10)
        
        # 创建情感按钮
        for emotion_key, emotion_name in EMOTIONS.items():
            btn = ttk.Button(
                right_frame, 
                text=emotion_name,
                command=lambda e=emotion_key: self.classify_current(e)
            )
            btn.pack(fill=tk.X, pady=2)
            # 绑定数字快捷键
            key = str(list(EMOTIONS.keys()).index(emotion_key) + 1)
            self.root.bind(key, lambda e, ek=emotion_key: self.classify_current(ek))
        
        # 导航按钮
        nav_frame = ttk.Frame(right_frame)
        nav_frame.pack(fill=tk.X, pady=20)
        
        ttk.Button(nav_frame, text="上一个 (←)", command=self.show_previous).pack(side=tk.LEFT, padx=5)
        ttk.Button(nav_frame, text="下一个 (→)", command=self.show_next).pack(side=tk.LEFT, padx=5)
        
        # 重复上次分类按钮
        self.repeat_btn = ttk.Button(
            right_frame,
            text="重复上次分类 (Space)",
            command=self.repeat_last_classification,
            state=tk.DISABLED
        )
        self.repeat_btn.pack(fill=tk.X, pady=10)
        
        # 保存并退出按钮
        ttk.Button(right_frame, text="保存并退出 (Q)", command=self.save_and_quit).pack(fill=tk.X, pady=10)
        
        # 绑定键盘事件
        self.root.bind('<Left>', lambda e: self.show_previous())
        self.root.bind('<Right>', lambda e: self.show_next())
        self.root.bind('<space>', lambda e: self.repeat_last_classification())
        self.root.bind('q', lambda e: self.save_and_quit())
        
        # 状态栏
        self.status_label = ttk.Label(main_frame, text="准备就绪")
        self.status_label.grid(row=1, column=0, columnspan=2, pady=5)

    def load_gif_files(self):
        gif_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs")
        self.gif_files = [f for f in os.listdir(gif_folder) if f.lower().endswith('.gif')]
        self.gif_files.sort()
        self.update_progress_label()

    def load_classifications(self):
        try:
            with open('emotion_classifications.json', 'r', encoding='utf-8') as f:
                self.classifications = json.load(f)
        except FileNotFoundError:
            self.classifications = {}

    def save_classifications(self):
        with open('emotion_classifications.json', 'w', encoding='utf-8') as f:
            json.dump(self.classifications, f, indent=4, ensure_ascii=False)

    def show_current_gif(self):
        if not self.gif_files:
            self.status_label.config(text="没有找到GIF文件")
            return

        current_file = self.gif_files[self.current_index]
        self.filename_label.config(text=current_file)
        self.update_progress_label()

        # 停止当前动画
        if self.animation is not None:
            self.image_label.after_cancel(self.animation)
            self.animation = None

        # 加载新的GIF
        gif_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs", current_file)
        self.animate_gif(gif_path)

        # 显示已有的分类结果
        if current_file in self.classifications:
            emotion = self.classifications[current_file]
            self.status_label.config(text=f"当前分类: {EMOTIONS[emotion]}")
        else:
            self.status_label.config(text="未分类")

    def animate_gif(self, gif_path):
        try:
            gif = Image.open(gif_path)
            frames = []
            
            try:
                while True:
                    current_frame = gif.copy()
                    current_frame.thumbnail((400, 400), Image.Resampling.LANCZOS)
                    frames.append(ImageTk.PhotoImage(current_frame))
                    gif.seek(gif.tell() + 1)
            except EOFError:
                pass

            if frames:
                self.show_frame(frames, 0, gif.info.get('duration', 100))

        except Exception as e:
            self.status_label.config(text=f"加载GIF出错: {str(e)}")
            self.image_label.config(image=None)

    def show_frame(self, frames, frame_index, duration):
        if not frames:
            return

        frame = frames[frame_index]
        self.image_label.config(image=frame)
        self.current_image = frame
        
        next_frame = (frame_index + 1) % len(frames)
        self.animation = self.root.after(duration, lambda: self.show_frame(frames, next_frame, duration))

    def classify_current(self, emotion):
        if not self.gif_files:
            return

        current_file = self.gif_files[self.current_index]
        self.classifications[current_file] = emotion
        self.last_emotion = emotion
        self.repeat_btn.config(state=tk.NORMAL)
        self.status_label.config(text=f"已将 {current_file} 分类为 {EMOTIONS[emotion]}")
        self.show_next()

    def repeat_last_classification(self):
        if self.last_emotion is not None:
            self.classify_current(self.last_emotion)

    def show_next(self):
        if self.current_index < len(self.gif_files) - 1:
            self.current_index += 1
            self.show_current_gif()

    def show_previous(self):
        if self.current_index > 0:
            self.current_index -= 1
            self.show_current_gif()

    def update_progress_label(self):
        total = len(self.gif_files)
        classified = len(self.classifications)
        self.progress_label.config(text=f"进度: {classified}/{total}")

    def save_and_quit(self):
        self.save_classifications()
        self.apply_classifications()
        self.root.quit()

    def apply_classifications(self):
        base_folder = os.path.join(os.path.dirname(os.path.dirname(__file__)), "dog_gifs")
        
        # 为每种情感创建子文件夹
        for emotion in EMOTIONS:
            emotion_folder = os.path.join(base_folder, emotion)
            os.makedirs(emotion_folder, exist_ok=True)

        # 移动文件到对应的情感文件夹
        for filename, emotion in self.classifications.items():
            source_path = os.path.join(base_folder, filename)
            dest_path = os.path.join(base_folder, emotion, filename)
            
            if os.path.exists(source_path):
                try:
                    shutil.move(source_path, dest_path)
                except Exception as e:
                    print(f"移动文件 {filename} 时出错: {e}")

if __name__ == "__main__":
    root = tk.Tk()
    app = EmotionClassifier(root)
    root.mainloop()
