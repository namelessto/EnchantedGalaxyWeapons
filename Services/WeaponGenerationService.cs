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

            if (r.NextDouble() <= _config.ChanceForGemEnchantment || _config.ForceGemEnchantment)
            {
                var gemTypes = GetAvailableGemTypes(weapon);
                if (gemTypes.Count > 0)
                {
                    int maxLevel = Math.Max(_config.MinGemLevel, _config.MaxGemLevel);
                    int count = _config.MinGemLevel >= maxLevel
                        ? _config.MinGemLevel
                        : r.Next(_config.MinGemLevel, maxLevel + 1);
                    count = Math.Clamp(count, 1, 3);

                    switch (_config.GemMode)
                    {
                        case GemApplicationMode.Stack:
                            // One gem type applied count times → Ruby×3
                            GemEnchantmentType stacked = gemTypes[r.Next(gemTypes.Count)];
                            for (int i = 0; i < count; i++)
                                weapon.AddEnchantment(CreateGemEnchantment(stacked));
                            break;

                        case GemApplicationMode.Different:
                            // count different gem types, each once → Ruby + Aquamarine + Jade
                            foreach (var type in gemTypes.OrderBy(_ => r.Next()).Take(count))
                                weapon.AddEnchantment(CreateGemEnchantment(type));
                            break;

                        case GemApplicationMode.Random:
                            // count random picks with replacement → Ruby×2 + Aquamarine
                            for (int i = 0; i < count; i++)
                                weapon.AddEnchantment(CreateGemEnchantment(gemTypes[r.Next(gemTypes.Count)]));
                            break;
                    }
                }
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
                if (_config.MinWeaponLevel > 0 && new MeleeWeapon(WeaponIds[type]).getItemLevel() < _config.MinWeaponLevel) continue;

                float w = _config.WeaponCategoryWeights.GetValueOrDefault(CategoryOf(type), 1f);
                if (w > 0f)
                    pool.Add((WeaponIds[type], w));
            }

            foreach (string id in GetModdedWeaponIds())
            {
                if (_config.MinWeaponLevel > 0 && new MeleeWeapon(id).getItemLevel() < _config.MinWeaponLevel) continue;

                float w = _config.WeaponCategoryWeights.GetValueOrDefault(CategoryOfId(id), 1f);
                if (w > 0f)
                    pool.Add((id, w));
            }

            if (_config.EnableAllVanillaWeapons && Game1.weaponData != null)
            {
                var inPool = pool.Select(e => e.id).ToHashSet();
                foreach (var (key, data) in Game1.weaponData)
                {
                    if (!int.TryParse(key, out _)) continue;         // vanilla only
                    if (inPool.Contains(key)) continue;               // no duplicates
                    if ((int)data.Type > 2) continue;                 // skip scythes
                    if (_config.MinWeaponLevel > 0 && new MeleeWeapon(key).getItemLevel() < _config.MinWeaponLevel) continue;

                    float w = _config.WeaponCategoryWeights.GetValueOrDefault(CategoryOfId(key), 1f);
                    if (w > 0f)
                        pool.Add((key, w));
                }
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

        private List<GemEnchantmentType> GetAvailableGemTypes(MeleeWeapon weapon)
        {
            var pool = new List<GemEnchantmentType>();
            foreach (var (type, enabled) in _config.AllowedGemEnchantments)
            {
                if (!enabled) continue;
                if (CreateGemEnchantment(type).CanApplyTo(weapon))
                    pool.Add(type);
            }
            return pool;
        }

        private static BaseEnchantment CreateGemEnchantment(GemEnchantmentType type) => type switch
        {
            GemEnchantmentType.Ruby       => new RubyEnchantment(),
            GemEnchantmentType.Aquamarine => new AquamarineEnchantment(),
            GemEnchantmentType.Jade       => new JadeEnchantment(),
            GemEnchantmentType.Amethyst   => new AmethystEnchantment(),
            GemEnchantmentType.Topaz      => new TopazEnchantment(),
            GemEnchantmentType.Emerald    => new EmeraldEnchantment(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

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
                    InnateType.CritPower    => new CritPowerEnchantment    { Level = Math.Max(1, Math.Min(3, r.Next(Math.Max(1, level)))) },
                    InnateType.CritChance   => new CritEnchantment         { Level = Math.Max(1, Math.Min(3, r.Next(Math.Max(1, level)))) },
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

            int actualMax  = Math.Min(_config.MaxInnateEnchantments, pool.Count);
            int lowerBound = Math.Min(_config.MinInnateEnchantments, actualMax);
            int count = r.Next(lowerBound, actualMax + 1);

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
            WeaponKeyHelper.HasModPrefix(weaponKey, modId);
    }
}
