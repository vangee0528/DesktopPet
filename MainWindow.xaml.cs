using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DesktopPet
{
    public partial class MainWindow : Window
    {
        private readonly Random random = new();
        private string[]? gifFiles;
        private DispatcherTimer? timer;
        private int currentIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // 获取所有GIF文件（使用绝对路径）
                string gifFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dog_gifs");
                if (!Directory.Exists(gifFolder))
                {
                    gifFolder = Path.Combine(Directory.GetCurrentDirectory(), "dog_gifs");
                }
                
                Console.WriteLine($"正在搜索GIF文件夹：{gifFolder}");
                
                if (!Directory.Exists(gifFolder))
                {
                    Console.WriteLine($"错误：GIF文件夹不存在：{gifFolder}");
                    Close();
                    return;
                }

                gifFiles = Directory.GetFiles(gifFolder, "*.gif");
                if (gifFiles.Length == 0)
                {
                    Console.WriteLine("错误：未找到任何GIF文件！");
                    Close();
                    return;
                }

                Console.WriteLine($"找到{gifFiles.Length}个GIF文件");

                // 创建定时器
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(10) // 默认10秒切换一次
                };
                timer.Tick += Timer_Tick;
                
                // 开始播放
                PlayRandomGif();
                timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：初始化失败：{ex.Message}");
                Close();
            }
        }

        private void PlayRandomGif()
        {
            try
            {
                if (gifFiles == null || timer == null) return;

                // 随机选择一个与当前不同的GIF
                int newIndex;
                do
                {
                    newIndex = random.Next(gifFiles.Length);
                } while (newIndex == currentIndex && gifFiles.Length > 1);

                currentIndex = newIndex;
                string gifPath = gifFiles[currentIndex];
                gifPlayer.Source = new Uri(gifPath, UriKind.Absolute);
                gifPlayer.Play();
                
                Console.WriteLine($"正在播放：{Path.GetFileName(gifPath)}");
                
                // 随机设置下一次切换的时间（5-15秒）
                timer.Interval = TimeSpan.FromSeconds(random.Next(5, 16));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：播放GIF失败：{ex.Message}");
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            PlayRandomGif();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 允许拖动窗口
            DragMove();
        }

        private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // GIF播放结束后重新开始播放
            gifPlayer.Position = TimeSpan.Zero;
            gifPlayer.Play();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}