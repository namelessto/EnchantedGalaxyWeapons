using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchantedGalaxyWeapons
{
    public sealed class ModConfig
    {
        public bool HaveDailySpawnLimit { get; set; } = true;
        public bool AlwaysHaveInnateEnchantment { get; set; } = false;
        public bool AlwaysHaveEnchantment { get; set; } = false;
        public bool SkipGalaxyCheck { get; set; } = false;
        public bool SkipInfinityCheck { get; set; } = false;
        public bool HaveGlobalChance { get; set; } = false;
        public bool AllowMoreThanOne { get; set; } = false;
        public int DailySpawnLimit { get; set; } = 2;
        public int AdditionalTriesToSpawn { get; set; } = 0;
        public int AdditionalBarrels { get; set; } = 0;
        public float RegularEnchantmentChance { get; set; } = 0.2f;
        public float BaseSpawnChance { get; set; } = 0.6f;
        public float IncreaseSpawnChanceStep { get; set; } = 0.05f;
        public float ChanceForEnchantment { get; set; } = 0.15f;

    }
}
