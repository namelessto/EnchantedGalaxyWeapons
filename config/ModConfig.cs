using EnchantedGalaxyWeapons.Core;
// using StardewModdingAPI.Utilities;

namespace EnchantedGalaxyWeapons.Config
{
    internal class ModConfig
    {
        // --- Dev tools ---
        // public KeybindList DevMenuKey { get; set; } = KeybindList.Parse("F6");
        // --- Custom loot table ---
        public bool EnableCustomLoot { get; set; } = true;
        public float CustomLootChance { get; set; } = 0.5f;
        public int LootMinStack { get; set; } = 1;
        public int LootMaxStack { get; set; } = 5;
        // Value-based pool: all (O) items with sell price in range are included automatically.
        // Set both to 0 to disable value filtering entirely.
        public int LootMinValue { get; set; } = 200;
        public int LootMaxValue { get; set; } = 0; // 0 = no upper cap
        // Manual list: explicit item IDs always added to the pool regardless of value.
        public List<string> LootItems { get; set; } =
        [
            "(O)749", // Omni Geode
            "(O)535", // Geode
            "(O)536", // Frozen Geode
            "(O)537", // Magma Geode
            "(O)386", // Iridium Ore
            "(O)337", // Iridium Bar
            "(O)384", // Gold Ore
            "(O)336", // Gold Bar
            "(O)72",  // Diamond
            "(O)64",  // Ruby
            "(O)60",  // Emerald
            "(O)62",  // Aquamarine
            "(O)66",  // Amethyst
            "(O)68",  // Topaz
            "(O)70",  // Jade
        ];
        // --- Spawn limits ---
        public bool HaveDailySpawnLimit { get; set; } = true;
        public int DailySpawnLimit { get; set; } = 5;

        // --- Spawn chance ---
        public bool HaveGlobalChance { get; set; } = false;
        public float BaseSpawnChance { get; set; } = 0.6f;
        public float IncreaseSpawnChanceStep { get; set; } = 0.05f;

        // --- Barrel count ---
        public int AdditionalBarrels { get; set; } = 0;
        public int AdditionalTriesToSpawn { get; set; } = 0;

        // --- Weapon unlock gates ---
        public bool SkipGalaxyCheck { get; set; } = false;
        public bool SkipInfinityCheck { get; set; } = false;
        public int MinWeaponLevel { get; set; } = 0;
        public bool EnableAllVanillaWeapons { get; set; } = false;

        // --- Enchantment chances ---
        public float ChanceForEnchantment { get; set; } = 0.15f;
        public float ChanceForInnate { get; set; } = 0.5f;
        public float ChanceForGemEnchantment { get; set; } = 0.1f;
        public bool ForceHaveEnchantment { get; set; } = false;
        public bool ForceInnateEnchantment { get; set; } = false;
        public bool ForceGemEnchantment { get; set; } = false;

        // --- Gem enchantment levels ---
        public int MinGemLevel { get; set; } = 1;
        public int MaxGemLevel { get; set; } = 3;
        public GemApplicationMode GemMode { get; set; } = GemApplicationMode.Stack;

        // --- Innate enchantment counts ---
        public bool KeepVanilla { get; set; } = true;
        public int MinInnateEnchantments { get; set; } = 1;
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
        // Tracks every prefix ever detected so opt-outs survive across save loads.
        public HashSet<string> SeenModWeaponPrefixes { get; set; } = new();

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

        // --- Allowed gem enchantments ---
        public Dictionary<GemEnchantmentType, bool> AllowedGemEnchantments { get; set; } = new()
        {
            { GemEnchantmentType.Ruby,       true },
            { GemEnchantmentType.Aquamarine, true },
            { GemEnchantmentType.Jade,       true },
            { GemEnchantmentType.Amethyst,   true },
            { GemEnchantmentType.Topaz,      true },
            { GemEnchantmentType.Emerald,    true }
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
