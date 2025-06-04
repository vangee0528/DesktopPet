using System.IO;
using System.Windows.Media.Imaging;

namespace DesktopPet
{
    public class PetBehavior
    {
        private readonly Dictionary<EmotionType, List<string>> emotionGifs = new();
        private readonly Random random = new();
        private readonly string baseGifPath;

        public PetBehavior()
        {
            baseGifPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dog_gifs");
            Console.WriteLine($"初始化PetBehavior，基础路径：{baseGifPath}");
            LoadGifs();
        }

        private void LoadGifs()
        {
            try
            {
                // 为每种情感加载对应文件夹中的GIF
                foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
                {
                    var emotionFolder = emotion.ToString().ToLower();
                    var folderPath = Path.Combine(baseGifPath, emotionFolder);
                    Console.WriteLine($"检查情感文件夹：{folderPath}");

                    if (Directory.Exists(folderPath))
                    {
                        var files = Directory.GetFiles(folderPath, "*.gif");
                        emotionGifs[emotion] = files.ToList();
                        Console.WriteLine($"已加载 {emotion} 类型的GIF：{files.Length}个");
                    }
                    else
                    {
                        Console.WriteLine($"警告：文件夹不存在 - {folderPath}");
                        emotionGifs[emotion] = new List<string>();
                    }
                }

                // 输出统计信息
                Console.WriteLine("GIF加载统计：");
                foreach (var kvp in emotionGifs)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value.Count}个文件");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载GIF文件时出错：{ex}");
            }
        }

        public string GetRandomGifForEmotion(EmotionType emotion)
        {
            if (emotionGifs.TryGetValue(emotion, out var gifs) && gifs.Any())
            {
                var selectedGif = gifs[random.Next(gifs.Count)];
                Console.WriteLine($"为{emotion}情绪选择GIF：{Path.GetFileName(selectedGif)}");
                return selectedGif;
            }

            Console.WriteLine($"警告：未找到{emotion}情绪的GIF，使用neutral类别");
            // 如果没有对应情感的GIF，返回neutral类别的随机GIF
            var neutralGifs = emotionGifs[EmotionType.Neutral];
            if (!neutralGifs.Any())
            {
                throw new InvalidOperationException("错误：没有可用的GIF文件");
            }
            return neutralGifs[random.Next(neutralGifs.Count)];
        }

        public BitmapImage LoadGifImage(string gifPath)
        {
            try
            {
                Console.WriteLine($"正在加载GIF：{Path.GetFileName(gifPath)}");
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(gifPath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze(); // 防止跨线程访问问题
                Console.WriteLine("GIF加载成功");
                return image;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载GIF失败：{ex}");
                throw;
            }
        }
    }
}
