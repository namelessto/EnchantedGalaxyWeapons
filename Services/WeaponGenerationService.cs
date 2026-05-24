using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace EnchantedGalaxyWeapons.Services
{
    internal sealed class WeaponGenerationService
    {
        private static readonly Dictionary<WeaponType, string> WeaponIds = new()
        {
            { WeaponType.GalaxySword,   "4"  },
            { WeaponType.GalaxyDagger,  "5"  },
            { WeaponType.GalaxyHammer,  "29" },
            { WeaponType.InfinitySword,  "62" },
            { WeaponType.InfinityDagger, "63" },
            { WeaponType.InfinityHammer, "64" }
        };

        private readonly ModConfig _config;

        public WeaponGenerationService(ModConfig config)
        {
            _config = config;
        }

        public MeleeWeapon? GenerateWeapon(Random r, bool galaxyUnlocked, bool infinityUnlocked)
        {
            var pool = GetWeaponsToChoose(galaxyUnlocked, infinityUnlocked);
            if (pool.Count == 0) return null;
            string? picked = PickWeighted(pool, r);
            if (picked == null) return null;
            MeleeWeapon weapon = new(picked);

            if (_config.KeepVanilla)
            {
                weapon = (MeleeWeapon)MeleeWeapon.attemptAddRandomInnateEnchantment(
                    weapon, r, force: _config.ForceInnateEnchantment);
            }
            else
            {
                weapon = AttemptAddInnateEnchantment(weapon, r, GetInnateEnchantments(weapon, r), _config.ForceInnateEnchantment);
            }

            if (r.NextDouble() <= _config.ChanceForEnchantment || _config.ForceHaveEnchantment)
            {
                var enchantments = GetEnchantments();
                if (enchantments.Count > 0)
                    weapon.AddEnchantment(enchantments[r.Next(enchantments.Count)]);
            }

            return weapon;
        }

        private List<(string id, float weight)> GetWeaponsToChoose(bool galaxyUnlocked, bool infinityUnlocked)
        {
            var pool = new List<(string id, float weight)>();

            foreach (var (type, enabled) in _config.AllowedWeapons)
            {
                if (!enabled) continue;

                bool isGalaxy = type is WeaponType.GalaxySword or WeaponType.GalaxyDagger or WeaponType.GalaxyHammer;
                bool isInfinity = type is WeaponType.InfinitySword or WeaponType.InfinityDagger or WeaponType.InfinityHammer;

                if (isGalaxy && !(_config.SkipGalaxyCheck || galaxyUnlocked)) continue;
                if (isInfinity && !(_config.SkipInfinityCheck || infinityUnlocked)) continue;

                float w = _config.WeaponCategoryWeights.GetValueOrDefault(CategoryOf(type), 1f);
                if (w > 0f)
                    pool.Add((WeaponIds[type], w));
            }

            foreach (string id in GetModdedWeaponIds())
            {
                float w = _config.WeaponCategoryWeights.GetValueOrDefault(CategoryOfId(id), 1f);
                if (w > 0f)
                    pool.Add((id, w));
            }

            return pool;
        }

        private static WeaponCategory CategoryOf(WeaponType type) => type switch
        {
            WeaponType.GalaxyDagger or WeaponType.InfinityDagger => WeaponCategory.Dagger,
            WeaponType.GalaxyHammer or WeaponType.InfinityHammer => WeaponCategory.Hammer,
            _ => WeaponCategory.Sword
        };

        private static WeaponCategory CategoryOfId(string id)
        {
            if (Game1.weaponData != null && Game1.weaponData.TryGetValue(id, out var data))
            {
                return (int)data.Type switch
                {
                    1 => WeaponCategory.Dagger,
                    2 => WeaponCategory.Hammer,
                    _ => WeaponCategory.Sword
                };
            }
            return WeaponCategory.Sword;
        }

        private static string? PickWeighted(List<(string id, float weight)> pool, Random r)
        {
            float total = pool.Sum(x => x.weight);
            if (total <= 0f) return null;

            float pick = (float)(r.NextDouble() * total);
            foreach (var (id, weight) in pool)
            {
                pick -= weight;
                if (pick <= 0f) return id;
            }
            return pool[^1].id;
        }

        private List<BaseEnchantment> GetEnchantments()
        {
            var pool = new List<BaseEnchantment>();
            foreach (var (type, enabled) in _config.AllowedEnchantments)
            {
                if (!enabled) continue;
                pool.Add(type switch
                {
                    EnchantmentType.Artful    => new ArtfulEnchantment(),
                    EnchantmentType.BugKiller => new BugKillerEnchantment(),
                    EnchantmentType.Crusader  => new CrusaderEnchantment(),
                    EnchantmentType.Haymaker  => new HaymakerEnchantment(),
                    EnchantmentType.Vampiric  => new VampiricEnchantment(),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                });
            }
            return pool;
        }

        private List<BaseEnchantment> GetInnateEnchantments(MeleeWeapon weapon, Random r)
        {
            int level = weapon.getItemLevel();
            var pool = new List<BaseEnchantment>();

            foreach (var (type, enabled) in _config.AllowedStats)
            {
                if (!enabled) continue;
                pool.Add(type switch
                {
                    InnateType.Defense      => new DefenseEnchantment      { Level = Math.Max(1, Math.Min(2, r.Next(level + 1) / 2 + 1)) },
                    InnateType.Weight       => new LightweightEnchantment  { Level = r.Next(1, 6) },
                    InnateType.SlimeGatherer => new SlimeGathererEnchantment(),
                    InnateType.SlimeSlayer  => new SlimeSlayerEnchantment(),
                    InnateType.CritPower    => new CritPowerEnchantment    { Level = Math.Max(1, Math.Min(3, r.Next(level) / 3)) },
                    InnateType.CritChance   => new CritEnchantment         { Level = Math.Max(1, Math.Min(3, r.Next(level) / 3)) },
                    InnateType.Attack       => new AttackEnchantment       { Level = Math.Max(1, Math.Min(5, r.Next(level + 1) / 2 + 1)) },
                    InnateType.Speed        => new WeaponSpeedEnchantment  { Level = Math.Max(1, Math.Min(4, r.Next(Math.Max(1, level)))) },
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                });
            }

            return pool;
        }

        private MeleeWeapon AttemptAddInnateEnchantment(MeleeWeapon weapon, Random r, List<BaseEnchantment> pool, bool force)
        {
            if (!force && r.NextDouble() > _config.ChanceForInnate)
                return weapon;
            if (pool.Count == 0)
                return weapon;

            int actualMax = Math.Min(_config.MaxInnateEnchantments, pool.Count);
            int count = r.Next(_config.MinInnateEnchantments, actualMax + 1);

            var available = pool.ToList();
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int idx = r.Next(available.Count);
                weapon.AddEnchantment(available[idx]);
                available.RemoveAt(idx);
            }

            return weapon;
        }

        private List<string> GetModdedWeaponIds()
        {
            if (_config.EnabledModWeaponIds.Count == 0 || Game1.weaponData == null)
                return new List<string>();

            return Game1.weaponData.Keys
                .Where(k => _config.EnabledModWeaponIds.Any(id => HasModPrefix(k, id)))
                .ToList();
        }

        private static bool HasModPrefix(string weaponKey, string modId) =>
            weaponKey.StartsWith(modId, StringComparison.OrdinalIgnoreCase) &&
            (weaponKey.Length == modId.Length || !char.IsLetterOrDigit(weaponKey[modId.Length]));
    }
}
