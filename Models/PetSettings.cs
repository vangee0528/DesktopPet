using System.IO;
using System.Text.Json;

namespace DesktopPet
{
    public class PetSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        // 动画设置
        public int DefaultAnimationInterval { get; set; } = 30;    // 默认动画切换间隔（秒）
        public int ExcitedAnimationInterval { get; set; } = 10;    // 兴奋状态下的切换间隔
        public int SleepyAnimationInterval { get; set; } = 45;     // 困倦状态下的切换间隔
        public int AngryAnimationInterval { get; set; } = 5;       // 生气状态下的切换间隔

        // 情感参数
        public double EnergyDecayRate { get; set; } = 1.0;        // 每分钟情感能量衰减值
        public double InteractionNeedRate { get; set; } = 1.0;    // 每分钟互动渴望增长值
        public double PetEnergyBoost { get; set; } = 15.0;       // 抚摸时情感能量提升值
        public int RapidClickThreshold { get; set; } = 5;        // 快速点击判定阈值
        public double RandomExcitementChance { get; set; } = 0.3; // 随机兴奋度变化概率

        // 时间阈值
        public int NightStartHour { get; set; } = 22;            // 夜间开始时间
        public int NightEndHour { get; set; } = 6;              // 夜间结束时间

        // 加载设置
        public static PetSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<PetSettings>(json);
                    return settings ?? new PetSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载设置失败：{ex.Message}");
            }
            return new PetSettings();
        }

        // 保存设置
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存设置失败：{ex.Message}");
            }
        }
    }
}
