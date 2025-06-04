using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopPet
{
    public enum EmotionType
    {
        Neutral,
        Happy,
        Excited,
        Curious,
        Sleepy,
        Disappointed,
        Sad,
        Angry
    }

    public class EmotionState
    {
        private static readonly Random random = new();
        private PetSettings settings;
        
        // 核心情感驱动因素
        public double EmotionEnergy { get; private set; } = 50;        // 情感能量
        public double InteractionNeed { get; private set; } = 0;       // 互动渴望
        public double RandomExcitement { get; private set; } = 50;     // 随机兴奋度
        
        // 当前情感状态
        public EmotionType CurrentEmotion { get; private set; } = EmotionType.Neutral;
        public DateTime LastEmotionChange { get; private set; } = DateTime.Now;
        public DateTime LastInteraction { get; private set; } = DateTime.Now;
        
        // 情感状态持续时间配置（秒）
        private readonly Dictionary<EmotionType, (int Min, int Max)> emotionDurations = new()
        {
            { EmotionType.Happy, (15, 30) },
            { EmotionType.Excited, (10, 20) },
            { EmotionType.Curious, (10, 15) },
            { EmotionType.Sleepy, (60, 120) },
            { EmotionType.Disappointed, (20, 40) },
            { EmotionType.Sad, (30, 60) },
            { EmotionType.Angry, (10, 15) }
        };

        public EmotionState(PetSettings settings)
        {
            this.settings = settings;
            LastEmotionChange = DateTime.Now;
            LastInteraction = DateTime.Now;
            CurrentEmotion = EmotionType.Neutral;
        }

        // 更新情感状态
        public void Update()
        {
            var now = DateTime.Now;
            var timeSinceLastUpdate = (now - LastEmotionChange).TotalSeconds;

            // 使用设置参数进行衰减
            EmotionEnergy = Math.Clamp(
                EmotionEnergy - (timeSinceLastUpdate / 60 * settings.EnergyDecayRate), 
                0, 100);
            
            // 使用设置参数增加互动渴望
            InteractionNeed = Math.Clamp(
                InteractionNeed + (timeSinceLastUpdate / 60 * settings.InteractionNeedRate), 
                0, 100);
            
            // 使用设置的概率进行随机兴奋度变化
            if (random.NextDouble() < settings.RandomExcitementChance)
            {
                RandomExcitement = Math.Clamp(
                    RandomExcitement + (random.NextDouble() * 40 - 20), 
                    0, 100);
            }

            DetermineNewEmotion();
        }

        // 添加更新设置的方法
        public void UpdateSettings(PetSettings newSettings)
        {
            settings = newSettings;
        }

        private void DetermineNewEmotion()
        {
            var timeSinceLastChange = (DateTime.Now - LastEmotionChange).TotalSeconds;
            
            if (CurrentEmotion != EmotionType.Neutral && 
                emotionDurations.TryGetValue(CurrentEmotion, out var duration))
            {
                // 如果当前情感状态未超过其持续时间，保持不变
                if (timeSinceLastChange < random.Next(duration.Min, duration.Max))
                {
                    return;
                }
            }

            // 根据各种因素决定新的情感状态
            var newEmotion = DetermineEmotionBasedOnFactors();

            if (newEmotion != CurrentEmotion)
            {
                CurrentEmotion = newEmotion;
                LastEmotionChange = DateTime.Now;
            }
        }

        private EmotionType DetermineEmotionBasedOnFactors()
        {
            // 按照情感状态决策逻辑判断
            if (EmotionEnergy < 30 || IsNightTime()) 
                return EmotionType.Sleepy;
            
            if (RandomExcitement > 80 || (EmotionEnergy > 60 && (DateTime.Now - LastInteraction).TotalSeconds < 5))
                return EmotionType.Excited;
            
            if (EmotionEnergy > 70 && InteractionNeed < 30)
                return EmotionType.Happy;
            
            if (InteractionNeed > 80 && EmotionEnergy < 40)
                return EmotionType.Sad;
            
            if (InteractionNeed > 70 && (DateTime.Now - LastInteraction).TotalHours > 2)
                return EmotionType.Disappointed;
            
            if (RandomExcitement > 70)
                return EmotionType.Curious;

            return EmotionType.Neutral;
        }

        // 交互响应方法
        public void OnPet()
        {
            EmotionEnergy = Math.Min(100, EmotionEnergy + 15);
            InteractionNeed = 0;
            LastInteraction = DateTime.Now;
            
            // 立即更新情感状态
            Update();
        }

        public void OnRapidClicks()
        {
            CurrentEmotion = EmotionType.Angry;
            LastEmotionChange = DateTime.Now;
        }

        private bool IsNightTime()
        {
            var hour = DateTime.Now.Hour;
            return hour >= settings.NightStartHour || hour < settings.NightEndHour;
        }
    }
}
