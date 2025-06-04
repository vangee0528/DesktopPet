using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using WpfAnimatedGif;

namespace DesktopPet
{
    public partial class FixedModeWindow : Window
    {
        private readonly PetSettings settings;
        private readonly List<string> availableGifs;
        private readonly List<string> selectedGifs;

        public FixedModeWindow(PetSettings settings)
        {
            InitializeComponent();
            this.settings = settings;
            
            // 初始化GIF列表
            availableGifs = new List<string>();
            selectedGifs = settings.FixedModeGifs?.ToList() ?? new List<string>();
            
            LoadGifLists();
            
            // 设置切换间隔
            IntervalBox.Text = settings.FixedModeSwitchInterval.ToString();
        }

        private void GifList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem is string selectedGif)
            {
                PreviewGif(selectedGif);
            }
        }

        private void LoadGifLists()
        {
            // 加载所有GIF文件
            var rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dog_gifs");
            foreach (var dir in Directory.GetDirectories(rootDir))
            {
                var emotionName = Path.GetFileName(dir);
                foreach (var file in Directory.GetFiles(dir, "*.gif"))
                {
                    // 使用包含情感文件夹的相对路径
                    var relativePath = Path.Combine(emotionName, Path.GetFileName(file));
                    if (!selectedGifs.Contains(relativePath))
                    {
                        availableGifs.Add(relativePath);
                    }
                }
            }

            // 更新列表显示并注册事件处理器
            UpdateListBoxes();
            
            // 绑定选择变更事件
            AvailableGifList.SelectionChanged += GifList_SelectionChanged;
            SelectedGifList.SelectionChanged += GifList_SelectionChanged;
        }

        private void UpdateListBoxes()
        {
            AvailableGifList.ItemsSource = null;
            AvailableGifList.ItemsSource = availableGifs.OrderBy(x => x);
            
            SelectedGifList.ItemsSource = null;
            SelectedGifList.ItemsSource = selectedGifs.OrderBy(x => x);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = AvailableGifList.SelectedItems.Cast<string>().ToList();
            if (!selected.Any()) return;

            foreach (var item in selected)
            {
                availableGifs.Remove(item);
                selectedGifs.Add(item);
            }

            UpdateListBoxes();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedGifList.SelectedItems.Cast<string>().ToList();
            if (!selected.Any()) return;

            foreach (var item in selected)
            {
                selectedGifs.Remove(item);
                availableGifs.Add(item);
            }

            UpdateListBoxes();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存选中的GIF列表
                settings.FixedModeGifs = selectedGifs.ToArray();
                
                // 保存切换间隔
                if (int.TryParse(IntervalBox.Text, out int interval))
                {
                    settings.FixedModeSwitchInterval = Math.Max(1, interval);
                }
                
                settings.Save();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败：{ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void EnableFixed_Click(object sender, RoutedEventArgs e)
        {
            if (selectedGifs.Count == 0)
            {
                MessageBox.Show("请先选择要显示的GIF！");
                return;
            }

            settings.IsFixedMode = true;
            settings.FixedModeGifs = selectedGifs.ToArray();
            
            if (int.TryParse(IntervalBox.Text, out int interval))
            {
                settings.FixedModeSwitchInterval = Math.Max(1, interval);
            }
            
            settings.Save();
            DialogResult = true;
            Close();
        }

        private void DisableFixed_Click(object sender, RoutedEventArgs e)
        {
            settings.IsFixedMode = false;
            settings.Save();
            DialogResult = true;
            Close();
        }

        private void PreviewGif(string gifName)
        {
            try
            {
                var gifPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dog_gifs", gifName);
                if (File.Exists(gifPath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(gifPath);
                    image.EndInit();
                    ImageBehavior.SetAnimatedSource(PreviewImage, image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"预览GIF失败：{ex.Message}");
            }
        }
    }
}
