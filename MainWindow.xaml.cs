using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfAnimatedGif;


namespace DesktopPet
{
    public partial class MainWindow : Window
    {
        private EmotionState? emotionState;
        private PetBehavior? petBehavior;
        private DispatcherTimer? emotionTimer;
        private DispatcherTimer? behaviorTimer;
        private DateTime lastClickTime = DateTime.MinValue;
        private int clickCount = 0;
        private Window? debugWindow;
        private PetSettings settings;

        public MainWindow()
        {
            try
            {
                Console.WriteLine("正在初始化主窗口...");
                InitializeComponent();
                
                // 加载设置
                settings = PetSettings.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化界面失败：{ex.Message}");
                throw;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("窗口加载完成，开始初始化系统...");
                
                // 初始化情感系统
                emotionState = new EmotionState(settings);
                petBehavior = new PetBehavior();

                // 情感更新定时器（每秒更新一次）
                emotionTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                emotionTimer.Tick += EmotionTimer_Tick;
                emotionTimer.Start();

                // 行为更新定时器（控制GIF切换，根据情感状态决定间隔）
                behaviorTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(settings.DefaultAnimationInterval)
                };
                behaviorTimer.Tick += BehaviorTimer_Tick;
                behaviorTimer.Start();

                // 显示初始状态
                UpdatePetDisplay();
                
                Console.WriteLine("系统初始化完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化系统失败：{ex.Message}\n\n{ex.StackTrace}");
                Close();
            }
        }

        private void EmotionTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                emotionState?.Update();
                UpdateBehaviorTimer();
                UpdateDebugInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"情感更新出错：{ex}");
            }
        }

        private void BehaviorTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                UpdatePetDisplay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"行为更新出错：{ex}");
            }
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow(settings)
                {
                    Owner = this
                };

                if (settingsWindow.ShowDialog() == true)
                {
                    // 更新情感系统的设置
                    emotionState?.UpdateSettings(settings);
                    
                    // 更新行为定时器
                    UpdateBehaviorTimer();
                    
                    // 立即更新显示
                    UpdatePetDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开设置失败：{ex.Message}");
            }
        }

        private void UpdateBehaviorTimer()
        {
            if (behaviorTimer == null || emotionState == null) return;

            // 根据当前情感状态设置定时器间隔
            var interval = emotionState.CurrentEmotion switch
            {
                EmotionType.Excited => settings.ExcitedAnimationInterval,
                EmotionType.Sleepy => settings.SleepyAnimationInterval,
                EmotionType.Angry => settings.AngryAnimationInterval,
                _ => settings.DefaultAnimationInterval
            };

            behaviorTimer.Interval = TimeSpan.FromSeconds(interval);
        }

        private void UpdatePetDisplay()
        {
            try
            {
                if (emotionState == null || petBehavior == null) return;

                Console.WriteLine($"更新显示，当前情绪：{emotionState.CurrentEmotion}");
                string gifPath = petBehavior.GetRandomGifForEmotion(emotionState.CurrentEmotion);
                var image = petBehavior.LoadGifImage(gifPath);
                ImageBehavior.SetAnimatedSource(displayImage, image);
                UpdateDebugInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新显示失败：{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();

            // 检测快速点击
            var now = DateTime.Now;
            if ((now - lastClickTime).TotalSeconds < 3)
            {
                clickCount++;
                if (clickCount > 5)
                {
                    Console.WriteLine("检测到快速点击，触发生气状态");
                    emotionState?.OnRapidClicks();
                    UpdatePetDisplay();
                    clickCount = 0;
                }
            }
            else
            {
                clickCount = 1;
            }
            lastClickTime = now;
        }

        private void DisplayImage_MouseEnter(object sender, MouseEventArgs e)
        {
            // TODO: 实现预互动状态的动画
            Console.WriteLine("鼠标进入区域");
        }

        private void DisplayImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Console.WriteLine("抚摸宠物");
                emotionState?.OnPet();
                UpdatePetDisplay();
            }
        }

        private void ShowDebugWindow_Click(object sender, RoutedEventArgs e)
        {
            if (debugWindow == null || !debugWindow.IsVisible)
            {
                debugWindow = new Window
                {
                    Title = "调试信息",
                    Width = 400,
                    Height = 300,
                    WindowStyle = WindowStyle.ToolWindow,
                    Content = new TextBox
                    {
                        IsReadOnly = true,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                };
                debugWindow.Show();
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo()
        {
            if (emotionState == null || behaviorTimer == null) return;

            if (debugWindow?.IsVisible == true && debugWindow.Content is TextBox textBox)
            {
                var info = $"当前状态 ({DateTime.Now:HH:mm:ss})\n" +
                          $"情绪: {emotionState.CurrentEmotion}\n" +
                          $"情感能量: {emotionState.EmotionEnergy:F1}\n" +
                          $"互动渴望: {emotionState.InteractionNeed:F1}\n" +
                          $"随机兴奋: {emotionState.RandomExcitement:F1}\n" +
                          $"行为间隔: {behaviorTimer.Interval.TotalSeconds:F1}秒";
                textBox.Text = info;
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // 保存设置
                settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败：{ex.Message}");
            }
        }
    }
}