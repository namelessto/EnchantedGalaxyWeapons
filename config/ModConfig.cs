using EnchantedGalaxyWeapons.Core;

namespace EnchantedGalaxyWeapons.Config
{
    internal class ModConfig
    {
        // --- Spawn limits ---
        public bool HaveDailySpawnLimit { get; set; } = true;
        public int DailySpawnLimit { get; set; } = 5;

        // --- Spawn chance ---
        public bool HaveGlobalChance { get; set; } = false;
        public float BaseSpawnChance { get; set; } = 0.6f;
        public float IncreaseSpawnChanceStep { get; set; } = 0.05f;

        // --- Barrel count ---
        public bool AllowMoreThanOne { get; set; } = false;
        public int AdditionalBarrels { get; set; } = 0;
        public int AdditionalTriesToSpawn { get; set; } = 0;

        // --- Weapon unlock gates ---
        public bool SkipGalaxyCheck { get; set; } = false;
        public bool SkipInfinityCheck { get; set; } = false;

        // --- Enchantment chances ---
        public float ChanceForEnchantment { get; set; } = 0.15f;
        public float ChanceForInnate { get; set; } = 0.5f;
        public bool ForceHaveEnchantment { get; set; } = false;
        public bool ForceInnateEnchantment { get; set; } = false;

        // --- Innate enchantment counts ---
        public bool KeepVanilla { get; set; } = true;
        public int MinInnateEnchantments { get; set; } = 0;
        public int MaxInnateEnchantments { get; set; } = 2;

        // --- Spawn location toggles ---
        public bool SpawnInLowerMines { get; set; } = true;
        public bool SpawnInMidMines { get; set; } = true;
        public bool SpawnInUpperMines { get; set; } = true;
        public bool SpawnInDangerousMines { get; set; } = true;
        public bool SpawnInSkullCavern { get; set; } = true;

        // --- Weapon category weights ---
        public Dictionary<WeaponCategory, float> WeaponCategoryWeights { get; set; } = new()
        {
            { WeaponCategory.Sword,   1f },
            { WeaponCategory.Dagger,  1f },
            { WeaponCategory.Hammer,  1f }
        };

        // --- Mod weapon integrations ---
        public HashSet<string> EnabledModWeaponIds { get; set; } = new();

        // --- Allowed weapons (which can drop) ---
        public Dictionary<WeaponType, bool> AllowedWeapons { get; set; } = new()
        {
            { WeaponType.GalaxySword, true },
            { WeaponType.GalaxyDagger, true },
            { WeaponType.GalaxyHammer, true },
            { WeaponType.InfinitySword, true },
            { WeaponType.InfinityDagger, true },
            { WeaponType.InfinityHammer, true }
        };

        // --- Allowed regular enchantments ---
        public Dictionary<EnchantmentType, bool> AllowedEnchantments { get; set; } = new()
        {
            { EnchantmentType.Artful, true },
            { EnchantmentType.BugKiller, true },
            { EnchantmentType.Crusader, true },
            { EnchantmentType.Haymaker, true },
            { EnchantmentType.Vampiric, true }
        };

        // --- Allowed innate stats ---
        public Dictionary<InnateType, bool> AllowedStats { get; set; } = new()
        {
            { InnateType.Defense, true },
            { InnateType.Weight, true },
            { InnateType.SlimeGatherer, true },
            { InnateType.SlimeSlayer, true },
            { InnateType.CritPower, true },
            { InnateType.CritChance, true },
            { InnateType.Attack, true },
            { InnateType.Speed, true }
        };
    }
}
