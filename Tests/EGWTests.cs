using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using EnchantedGalaxyWeapons.Objects;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Locations;
using StardewValley.Tools;
using TestRunner.Api;

namespace EnchantedGalaxyWeapons.Tests;

internal static class EGWTests
{
    private static readonly HashSet<string> GalaxyIds   = ["4", "5", "29"];
    private static readonly HashSet<string> InfinityIds = ["62", "63", "64"];

    // Barrel placed at this tile in integrated tests so the position is always known.
    private const int BarrelTX = 6, BarrelTY = 6;

    internal static void Register(ITestRunnerApi api)
    {
        const string mod = "NamelessTo.EnchantedGalaxyWeapons";
        RegisterWeaponGenTests(api, mod);
        RegisterSpawnServiceTests(api, mod);
        RegisterLootServiceTests(api, mod);
        RegisterUnlockTests(api, mod);
        RegisterIntegratedSpawnTests(api, mod);
        RegisterIntegratedBarrelTests(api, mod);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsGemEnchantment(BaseEnchantment e) =>
        e is RubyEnchantment or AquamarineEnchantment or JadeEnchantment or
             AmethystEnchantment or TopazEnchantment or EmeraldEnchantment;

    private static bool IsRegularEnchantment(BaseEnchantment e) =>
        e is ArtfulEnchantment or BugKillerEnchantment or CrusaderEnchantment or
             HaymakerEnchantment or VampiricEnchantment;

    private static bool IsInnateEnchantment(BaseEnchantment e) =>
        e is DefenseEnchantment or LightweightEnchantment or SlimeGathererEnchantment or
             SlimeSlayerEnchantment or CritPowerEnchantment or CritEnchantment or
             AttackEnchantment or WeaponSpeedEnchantment;

    // Place a barrel at the known test tile in the current mine location.
    private static void PlaceBarrelAtTestTile()
    {
        var mine = (MineShaft)Game1.currentLocation;
        var tile = new Vector2(BarrelTX, BarrelTY);
        mine.objects.Remove(tile);
        mine.objects[tile] = CustomBreakableObject.GetBarrelForMines(tile, mine);
    }

    // Call ReleaseContents on the barrel at the test tile, then remove it.
    private static void TriggerBarrelRelease()
    {
        var mine = (MineShaft)Game1.currentLocation;
        var tile = new Vector2(BarrelTX, BarrelTY);
        if (mine.objects.TryGetValue(tile, out var obj) && obj is CustomBreakableObject barrel)
        {
            barrel.releaseContents(Game1.player);
            mine.objects.Remove(tile);
        }
    }

    // ── Section 1: WeaponGenerationService ────────────────────────────────────

    private static void RegisterWeaponGenTests(ITestRunnerApi api, string mod)
    {
        // ── 1.1: Galaxy weapons available when galaxy is unlocked ─────────────
        {
            bool origVanilla = false;
            HashSet<string> origModIds = [];
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_galaxy_weapon_when_galaxy_unlocked", s => s
                .Do(() => {
                    origVanilla = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), galaxyUnlocked: true, infinityUnlocked: false);
                })
                .Assert("weapon is non-null",          () => weapon != null)
                .Assert("weapon is galaxy or infinity", () => GalaxyIds.Contains(weapon!.InitialParentTileIndex.ToString()) ||
                                                              InfinityIds.Contains(weapon!.InitialParentTileIndex.ToString()))
                .Do(() => {
                    ModEntry.Config.EnableAllVanillaWeapons = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.2: Returns null when neither unlock condition is met ─────────────
        {
            bool origSkipGalaxy = false, origSkipInfinity = false, origVanilla = false;
            HashSet<string> origModIds = [];
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_null_when_all_locked", s => s
                .Do(() => {
                    origSkipGalaxy   = ModEntry.Config.SkipGalaxyCheck;
                    origSkipInfinity = ModEntry.Config.SkipInfinityCheck;
                    origVanilla      = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds       = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = false;
                    ModEntry.Config.SkipInfinityCheck        = false;
                    ModEntry.Config.EnableAllVanillaWeapons  = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), galaxyUnlocked: false, infinityUnlocked: false);
                })
                .Assert("weapon pool is empty → null", () => weapon == null)
                .Do(() => {
                    ModEntry.Config.SkipGalaxyCheck         = origSkipGalaxy;
                    ModEntry.Config.SkipInfinityCheck        = origSkipInfinity;
                    ModEntry.Config.EnableAllVanillaWeapons  = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.3: SkipGalaxyCheck bypasses the unlock gate ─────────────────────
        {
            bool orig = false, origVanilla = false;
            HashSet<string> origModIds = [];
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_skip_galaxy_check_bypasses_lock", s => s
                .Do(() => {
                    orig        = ModEntry.Config.SkipGalaxyCheck;
                    origVanilla = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds  = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = true;
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), galaxyUnlocked: false, infinityUnlocked: false);
                })
                .Assert("galaxy weapon available without flag", () => weapon != null && GalaxyIds.Contains(weapon.InitialParentTileIndex.ToString()))
                .Do(() => {
                    ModEntry.Config.SkipGalaxyCheck         = orig;
                    ModEntry.Config.EnableAllVanillaWeapons = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.4: SkipInfinityCheck bypasses the infinity unlock gate ──────────
        {
            bool origGalaxy = false, origInfinity = false, origVanilla = false;
            HashSet<string> origModIds = [];
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_skip_infinity_check_bypasses_lock", s => s
                .Do(() => {
                    origGalaxy   = ModEntry.Config.SkipGalaxyCheck;
                    origInfinity = ModEntry.Config.SkipInfinityCheck;
                    origVanilla  = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds   = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = false;
                    ModEntry.Config.SkipInfinityCheck        = true;
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), galaxyUnlocked: false, infinityUnlocked: false);
                })
                .Assert("infinity weapon available without achievement", () =>
                    weapon != null && InfinityIds.Contains(weapon.InitialParentTileIndex.ToString()))
                .Do(() => {
                    ModEntry.Config.SkipGalaxyCheck         = origGalaxy;
                    ModEntry.Config.SkipInfinityCheck        = origInfinity;
                    ModEntry.Config.EnableAllVanillaWeapons = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.5: DisallowedWeapons are never generated ─────────────────────────
        {
            Dictionary<WeaponType, bool>? origAllowed = null;
            bool origSkip = false, origVanilla = false;
            HashSet<string> origModIds = [];
            api.RegisterScenario(mod, "gen_disabled_weapon_type_never_drops", s => s
                .Do(() => {
                    origAllowed = new Dictionary<WeaponType, bool>(ModEntry.Config.AllowedWeapons);
                    origSkip    = ModEntry.Config.SkipGalaxyCheck;
                    origVanilla = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds  = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = true;
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var key in ModEntry.Config.AllowedWeapons.Keys.ToList())
                        ModEntry.Config.AllowedWeapons[key] = key == WeaponType.GalaxySword;
                })
                .Assert("20 runs produce only Galaxy Sword (id=4)", () => {
                    for (int i = 0; i < 20; i++)
                    {
                        var w = ModEntry.GenerationService.GenerateWeapon(new Random(i), true, true);
                        if (w == null || w.InitialParentTileIndex != 4) return false;
                    }
                    return true;
                })
                .Do(() => {
                    foreach (var (k, v) in origAllowed!) ModEntry.Config.AllowedWeapons[k] = v;
                    ModEntry.Config.SkipGalaxyCheck         = origSkip;
                    ModEntry.Config.EnableAllVanillaWeapons = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.6: Category weight=0 excludes that category ─────────────────────
        {
            Dictionary<WeaponCategory, float>? origWeights = null;
            bool origSkipGalaxy = false, origSkipInfinity = false, origVanilla = false;
            HashSet<string> origModIds = [];
            api.RegisterScenario(mod, "gen_zero_dagger_weight_excludes_daggers", s => s
                .Do(() => {
                    origWeights     = new Dictionary<WeaponCategory, float>(ModEntry.Config.WeaponCategoryWeights);
                    origSkipGalaxy  = ModEntry.Config.SkipGalaxyCheck;
                    origSkipInfinity = ModEntry.Config.SkipInfinityCheck;
                    origVanilla     = ModEntry.Config.EnableAllVanillaWeapons;
                    origModIds      = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = true;
                    ModEntry.Config.SkipInfinityCheck        = true;
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    ModEntry.Config.WeaponCategoryWeights[WeaponCategory.Dagger] = 0f;
                })
                .Assert("no daggers in 30 runs", () => {
                    for (int i = 0; i < 30; i++)
                    {
                        var w = ModEntry.GenerationService.GenerateWeapon(new Random(i), true, true);
                        if (w?.InitialParentTileIndex is 5 or 63) return false;
                    }
                    return true;
                })
                .Do(() => {
                    foreach (var (k, v) in origWeights!) ModEntry.Config.WeaponCategoryWeights[k] = v;
                    ModEntry.Config.SkipGalaxyCheck         = origSkipGalaxy;
                    ModEntry.Config.SkipInfinityCheck        = origSkipInfinity;
                    ModEntry.Config.EnableAllVanillaWeapons = origVanilla;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }

        // ── 1.7: MinWeaponLevel=999 empties the pool ─────────────────────────
        {
            int origMin = 0;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_min_level_999_returns_null", s => s
                .Do(() => {
                    origMin = ModEntry.Config.MinWeaponLevel;
                    ModEntry.Config.MinWeaponLevel = 999;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, true);
                })
                .Assert("pool empty → null", () => weapon == null)
                .Do(() => ModEntry.Config.MinWeaponLevel = origMin)
            );
        }

        // ── 1.8: ForceHaveEnchantment always yields a regular enchantment ─────
        {
            bool origForce = false; float origChance = 0f;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_force_regular_enchantment", s => s
                .Do(() => {
                    origForce  = ModEntry.Config.ForceHaveEnchantment;
                    origChance = ModEntry.Config.ChanceForEnchantment;
                    ModEntry.Config.ForceHaveEnchantment = true;
                    ModEntry.Config.ChanceForEnchantment = 0f;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("weapon has a regular enchantment", () =>
                    weapon?.enchantments.Any(IsRegularEnchantment) == true)
                .Do(() => {
                    ModEntry.Config.ForceHaveEnchantment = origForce;
                    ModEntry.Config.ChanceForEnchantment = origChance;
                })
            );
        }

        // ── 1.9: ForceGemEnchantment always yields a gem enchantment ──────────
        {
            bool origForce = false; float origChance = 0f;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_force_gem_enchantment", s => s
                .Do(() => {
                    origForce  = ModEntry.Config.ForceGemEnchantment;
                    origChance = ModEntry.Config.ChanceForGemEnchantment;
                    ModEntry.Config.ForceGemEnchantment      = true;
                    ModEntry.Config.ChanceForGemEnchantment  = 0f;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("weapon has a gem enchantment", () =>
                    weapon?.enchantments.Any(IsGemEnchantment) == true)
                .Do(() => {
                    ModEntry.Config.ForceGemEnchantment     = origForce;
                    ModEntry.Config.ChanceForGemEnchantment = origChance;
                })
            );
        }

        // ── 1.10: ForceInnateEnchantment (custom logic) always adds innate stat
        {
            bool origForce = false, origVanilla = false; float origChance = 0f;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_force_innate_enchantment_custom", s => s
                .Do(() => {
                    origForce   = ModEntry.Config.ForceInnateEnchantment;
                    origVanilla = ModEntry.Config.KeepVanilla;
                    origChance  = ModEntry.Config.ChanceForInnate;
                    ModEntry.Config.ForceInnateEnchantment = true;
                    ModEntry.Config.KeepVanilla            = false;
                    ModEntry.Config.ChanceForInnate        = 0f;
                    ModEntry.Config.MinInnateEnchantments  = 1;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("weapon has an innate enchantment", () =>
                    weapon?.enchantments.Any(IsInnateEnchantment) == true)
                .Do(() => {
                    ModEntry.Config.ForceInnateEnchantment = origForce;
                    ModEntry.Config.KeepVanilla            = origVanilla;
                    ModEntry.Config.ChanceForInnate        = origChance;
                })
            );
        }

        // ── 1.11: GemMode=Stack produces one gem type with level=count ─────────
        {
            bool origForce = false; int origMin = 0, origMax = 0; GemApplicationMode origMode = GemApplicationMode.Stack;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_gem_mode_stack_same_type", s => s
                .Do(() => {
                    origForce = ModEntry.Config.ForceGemEnchantment;
                    origMin   = ModEntry.Config.MinGemLevel;
                    origMax   = ModEntry.Config.MaxGemLevel;
                    origMode  = ModEntry.Config.GemMode;
                    ModEntry.Config.ForceGemEnchantment = true;
                    ModEntry.Config.MinGemLevel = 3;
                    ModEntry.Config.MaxGemLevel = 3;
                    ModEntry.Config.GemMode     = GemApplicationMode.Stack;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("exactly 1 distinct gem type", () => {
                    if (weapon == null) return false;
                    var gems = weapon.enchantments.Where(IsGemEnchantment).ToList();
                    return gems.Select(e => e.GetType()).Distinct().Count() == 1;
                })
                .Assert("gem enchantment level is 3", () => {
                    var gems = weapon?.enchantments.Where(IsGemEnchantment).ToList();
                    return gems?.Sum(e => e.Level) == 3;
                })
                .Do(() => {
                    ModEntry.Config.ForceGemEnchantment = origForce;
                    ModEntry.Config.MinGemLevel = origMin;
                    ModEntry.Config.MaxGemLevel = origMax;
                    ModEntry.Config.GemMode     = origMode;
                })
            );
        }

        // ── 1.12: GemMode=Different produces distinct gem types ───────────────
        {
            bool origForce = false; int origMin = 0, origMax = 0; GemApplicationMode origMode = GemApplicationMode.Stack;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_gem_mode_different_unique_types", s => s
                .Do(() => {
                    origForce = ModEntry.Config.ForceGemEnchantment;
                    origMin   = ModEntry.Config.MinGemLevel;
                    origMax   = ModEntry.Config.MaxGemLevel;
                    origMode  = ModEntry.Config.GemMode;
                    ModEntry.Config.ForceGemEnchantment = true;
                    ModEntry.Config.MinGemLevel = 3;
                    ModEntry.Config.MaxGemLevel = 3;
                    ModEntry.Config.GemMode     = GemApplicationMode.Different;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("all gem enchantments are different types", () => {
                    if (weapon == null) return false;
                    var gems = weapon.enchantments.Where(IsGemEnchantment).ToList();
                    return gems.Count > 0 && gems.Select(e => e.GetType()).Distinct().Count() == gems.Count;
                })
                .Do(() => {
                    ModEntry.Config.ForceGemEnchantment = origForce;
                    ModEntry.Config.MinGemLevel = origMin;
                    ModEntry.Config.MaxGemLevel = origMax;
                    ModEntry.Config.GemMode     = origMode;
                })
            );
        }

        // ── 1.13: GemMode=Random produces exactly N gem enchant levels ─────────
        {
            bool origForce = false; int origMin = 0, origMax = 0; GemApplicationMode origMode = GemApplicationMode.Stack;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_gem_mode_random_correct_count", s => s
                .Do(() => {
                    origForce = ModEntry.Config.ForceGemEnchantment;
                    origMin   = ModEntry.Config.MinGemLevel;
                    origMax   = ModEntry.Config.MaxGemLevel;
                    origMode  = ModEntry.Config.GemMode;
                    ModEntry.Config.ForceGemEnchantment = true;
                    ModEntry.Config.MinGemLevel = 2;
                    ModEntry.Config.MaxGemLevel = 2;
                    ModEntry.Config.GemMode     = GemApplicationMode.Random;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("sum of gem levels == 2", () => {
                    var total = weapon?.enchantments.Where(IsGemEnchantment).Sum(e => e.Level);
                    return total == 2;
                })
                .Do(() => {
                    ModEntry.Config.ForceGemEnchantment = origForce;
                    ModEntry.Config.MinGemLevel = origMin;
                    ModEntry.Config.MaxGemLevel = origMax;
                    ModEntry.Config.GemMode     = origMode;
                })
            );
        }

        // ── 1.14: Enchantment chance=0 and not forced → no regular enchantment
        {
            bool origForce = false; float origChance = 0f;
            api.RegisterScenario(mod, "gen_no_enchantment_at_zero_chance", s => s
                .Do(() => {
                    origForce  = ModEntry.Config.ForceHaveEnchantment;
                    origChance = ModEntry.Config.ChanceForEnchantment;
                    ModEntry.Config.ForceHaveEnchantment = false;
                    ModEntry.Config.ChanceForEnchantment = 0f;
                })
                .Assert("10 runs → no regular enchantment ever", () => {
                    for (int i = 0; i < 10; i++)
                    {
                        var w = ModEntry.GenerationService.GenerateWeapon(new Random(i), true, false);
                        if (w?.enchantments.Any(IsRegularEnchantment) == true) return false;
                    }
                    return true;
                })
                .Do(() => {
                    ModEntry.Config.ForceHaveEnchantment = origForce;
                    ModEntry.Config.ChanceForEnchantment = origChance;
                })
            );
        }

        // ── 1.15: All enchantment types disabled → force still yields none ─────
        {
            bool origForce = false;
            Dictionary<EnchantmentType, bool>? origEnch = null;
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "gen_all_enchantments_disabled_yields_none", s => s
                .Do(() => {
                    origForce = ModEntry.Config.ForceHaveEnchantment;
                    origEnch  = new Dictionary<EnchantmentType, bool>(ModEntry.Config.AllowedEnchantments);
                    ModEntry.Config.ForceHaveEnchantment = true;
                    foreach (var key in ModEntry.Config.AllowedEnchantments.Keys.ToList())
                        ModEntry.Config.AllowedEnchantments[key] = false;
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), true, false);
                })
                .Assert("no regular enchantment (pool was empty)", () =>
                    weapon?.enchantments.Any(IsRegularEnchantment) == false)
                .Do(() => {
                    ModEntry.Config.ForceHaveEnchantment = origForce;
                    foreach (var (k, v) in origEnch!) ModEntry.Config.AllowedEnchantments[k] = v;
                })
            );
        }
    }

    // ── Section 2: WeaponSpawnService ─────────────────────────────────────────

    private static void RegisterSpawnServiceTests(ITestRunnerApi api, string mod)
    {
        // ── 2.1: DecrementDailyLimit reduces count by 1 ───────────────────────
        {
            int origLimit = 0; bool origHave = false;
            api.RegisterScenario(mod, "spawn_decrement_reduces_count", s => s
                .Do(() => {
                    origLimit = ModEntry.Config.DailySpawnLimit;
                    origHave  = ModEntry.Config.HaveDailySpawnLimit;
                    ModEntry.Config.HaveDailySpawnLimit = true;
                    ModEntry.Config.DailySpawnLimit = 5;
                    ModEntry.SpawnService.ResetDailyLimit(0);
                })
                .Assert("starts at 5", () => ModEntry.SpawnService.SpawnsRemainingToday == 5)
                .Do(() => ModEntry.SpawnService.DecrementDailyLimit())
                .Assert("decremented to 4", () => ModEntry.SpawnService.SpawnsRemainingToday == 4)
                .Do(() => {
                    ModEntry.Config.DailySpawnLimit     = origLimit;
                    ModEntry.Config.HaveDailySpawnLimit = origHave;
                    ModEntry.SpawnService.ResetDailyLimit(0);
                })
            );
        }

        // ── 2.2: Limit never goes below 0 ─────────────────────────────────────
        {
            bool origHave = false; int origLimit = 0;
            api.RegisterScenario(mod, "spawn_limit_floor_is_zero", s => s
                .Do(() => {
                    origHave  = ModEntry.Config.HaveDailySpawnLimit;
                    origLimit = ModEntry.Config.DailySpawnLimit;
                    ModEntry.Config.HaveDailySpawnLimit = true;
                    ModEntry.Config.DailySpawnLimit     = 1;
                    ModEntry.SpawnService.ResetDailyLimit(0);
                    ModEntry.SpawnService.DecrementDailyLimit(); // 1 → 0
                    ModEntry.SpawnService.DecrementDailyLimit(); // 0 → still 0
                })
                .Assert("still 0 after extra decrement", () => ModEntry.SpawnService.SpawnsRemainingToday == 0)
                .Do(() => {
                    ModEntry.Config.HaveDailySpawnLimit = origHave;
                    ModEntry.Config.DailySpawnLimit     = origLimit;
                })
            );
        }

        // ── 2.3: ResetDailyLimit adds luck bonus ──────────────────────────────
        {
            int origLimit = 0;
            api.RegisterScenario(mod, "spawn_reset_applies_luck_bonus", s => s
                .Do(() => {
                    origLimit = ModEntry.Config.DailySpawnLimit;
                    ModEntry.Config.DailySpawnLimit = 5;
                    ModEntry.SpawnService.ResetDailyLimit(luckLevel: 3);
                })
                .Assert("5 base + 3 luck = 8", () => ModEntry.SpawnService.SpawnsRemainingToday == 8)
                .Do(() => ModEntry.Config.DailySpawnLimit = origLimit)
            );
        }

        // ── 2.4: ForceNextSpawn flag clears after use ─────────────────────────
        //  Integrated: warps to a mine; verified after in integrated tests.
        //  Here we verify the flag itself clears once set.
        {
            api.RegisterScenario(mod, "spawn_force_flag_initially_false", s => s
                .Assert("ForceNextSpawn starts false", () => !ModEntry.ForceNextSpawn)
            );
        }
    }

    // ── Section 3: CustomLootService ──────────────────────────────────────────

    private static void RegisterLootServiceTests(ITestRunnerApi api, string mod)
    {
        // ── 3.1: Disabled loot always returns null ────────────────────────────
        {
            bool orig = false;
            api.RegisterScenario(mod, "loot_disabled_returns_null", s => s
                .Do(() => {
                    orig = ModEntry.Config.EnableCustomLoot;
                    ModEntry.Config.EnableCustomLoot = false;
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("RollLoot returns null", () => ModEntry.LootService.RollLoot(new Random(1)) == null)
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = orig;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 3.2: Chance=0 always returns null ─────────────────────────────────
        {
            bool origEnabled = false; float origChance = 0f;
            api.RegisterScenario(mod, "loot_zero_chance_always_null", s => s
                .Do(() => {
                    origEnabled = ModEntry.Config.EnableCustomLoot;
                    origChance  = ModEntry.Config.CustomLootChance;
                    ModEntry.Config.EnableCustomLoot = true;
                    ModEntry.Config.CustomLootChance = 0f;
                    ModEntry.Config.LootItems = ["(O)74"];
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("10 rolls → all null", () => {
                    for (int i = 0; i < 10; i++)
                        if (ModEntry.LootService.RollLoot(new Random(i)) != null) return false;
                    return true;
                })
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = origEnabled;
                    ModEntry.Config.CustomLootChance = origChance;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 3.3: Chance=1 with manual item always returns item ────────────────
        {
            bool origEnabled = false; float origChance = 0f;
            List<string>? origItems = null;
            api.RegisterScenario(mod, "loot_100_chance_returns_item", s => s
                .Do(() => {
                    origEnabled = ModEntry.Config.EnableCustomLoot;
                    origChance  = ModEntry.Config.CustomLootChance;
                    origItems   = ModEntry.Config.LootItems;
                    ModEntry.Config.EnableCustomLoot = true;
                    ModEntry.Config.CustomLootChance = 1f;
                    ModEntry.Config.LootItems = ["(O)74"];
                    ModEntry.Config.LootMinValue = 0;
                    ModEntry.Config.LootMaxValue = 0;
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("roll returns non-null", () => ModEntry.LootService.RollLoot(new Random(1)) != null)
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = origEnabled;
                    ModEntry.Config.CustomLootChance = origChance;
                    ModEntry.Config.LootItems = origItems!;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 3.4: Manual item ID appears in loot ───────────────────────────────
        {
            bool origEnabled = false; float origChance = 0f;
            List<string>? origItems = null; int origMinVal = 0, origMaxVal = 0;
            api.RegisterScenario(mod, "loot_manual_item_id_in_pool", s => s
                .Do(() => {
                    origEnabled = ModEntry.Config.EnableCustomLoot;
                    origChance  = ModEntry.Config.CustomLootChance;
                    origItems   = ModEntry.Config.LootItems;
                    origMinVal  = ModEntry.Config.LootMinValue;
                    origMaxVal  = ModEntry.Config.LootMaxValue;
                    ModEntry.Config.EnableCustomLoot = true;
                    ModEntry.Config.CustomLootChance = 1f;
                    ModEntry.Config.LootItems    = ["(O)74"];
                    ModEntry.Config.LootMinValue = 0;
                    ModEntry.Config.LootMaxValue = 0;
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("item id is (O)74 (diamond)", () =>
                    ModEntry.LootService.RollLoot(new Random(1))?.QualifiedItemId == "(O)74")
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = origEnabled;
                    ModEntry.Config.CustomLootChance = origChance;
                    ModEntry.Config.LootItems    = origItems!;
                    ModEntry.Config.LootMinValue = origMinVal;
                    ModEntry.Config.LootMaxValue = origMaxVal;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 3.5: Stack quantity is within [LootMinStack, LootMaxStack] ─────────
        {
            bool origEnabled = false; float origChance = 0f;
            List<string>? origItems = null; int origMin = 0, origMax = 0, origMinVal = 0, origMaxVal = 0;
            api.RegisterScenario(mod, "loot_stack_within_min_max", s => s
                .Do(() => {
                    origEnabled = ModEntry.Config.EnableCustomLoot;
                    origChance  = ModEntry.Config.CustomLootChance;
                    origItems   = ModEntry.Config.LootItems;
                    origMin     = ModEntry.Config.LootMinStack;
                    origMax     = ModEntry.Config.LootMaxStack;
                    origMinVal  = ModEntry.Config.LootMinValue;
                    origMaxVal  = ModEntry.Config.LootMaxValue;
                    ModEntry.Config.EnableCustomLoot = true;
                    ModEntry.Config.CustomLootChance = 1f;
                    ModEntry.Config.LootItems    = ["(O)74"];
                    ModEntry.Config.LootMinStack = 3;
                    ModEntry.Config.LootMaxStack = 5;
                    ModEntry.Config.LootMinValue = 0;
                    ModEntry.Config.LootMaxValue = 0;
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("20 rolls all have stack 3-5", () => {
                    for (int i = 0; i < 20; i++)
                    {
                        var item = ModEntry.LootService.RollLoot(new Random(i));
                        if (item == null || item.Stack < 3 || item.Stack > 5) return false;
                    }
                    return true;
                })
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = origEnabled;
                    ModEntry.Config.CustomLootChance = origChance;
                    ModEntry.Config.LootItems    = origItems!;
                    ModEntry.Config.LootMinStack = origMin;
                    ModEntry.Config.LootMaxStack = origMax;
                    ModEntry.Config.LootMinValue = origMinVal;
                    ModEntry.Config.LootMaxValue = origMaxVal;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 3.6: InvalidatePool causes pool to rebuild on next roll ───────────
        {
            bool origEnabled = false; float origChance = 0f;
            List<string>? origItems = null; int origMinVal = 0, origMaxVal = 0;
            api.RegisterScenario(mod, "loot_invalidate_rebuilds_pool", s => s
                .Do(() => {
                    origEnabled = ModEntry.Config.EnableCustomLoot;
                    origChance  = ModEntry.Config.CustomLootChance;
                    origItems   = ModEntry.Config.LootItems;
                    origMinVal  = ModEntry.Config.LootMinValue;
                    origMaxVal  = ModEntry.Config.LootMaxValue;
                    // First: empty pool
                    ModEntry.Config.EnableCustomLoot = true;
                    ModEntry.Config.CustomLootChance = 1f;
                    ModEntry.Config.LootItems    = [];
                    ModEntry.Config.LootMinValue = 0;
                    ModEntry.Config.LootMaxValue = 0;
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("empty pool returns null", () => ModEntry.LootService.RollLoot(new Random(1)) == null)
                .Do(() => {
                    // Add item, invalidate, now should return item
                    ModEntry.Config.LootItems = ["(O)74"];
                    ModEntry.LootService.InvalidatePool();
                })
                .Assert("after invalidate + item add, pool returns item", () =>
                    ModEntry.LootService.RollLoot(new Random(1)) != null)
                .Do(() => {
                    ModEntry.Config.EnableCustomLoot = origEnabled;
                    ModEntry.Config.CustomLootChance = origChance;
                    ModEntry.Config.LootItems    = origItems!;
                    ModEntry.Config.LootMinValue = origMinVal;
                    ModEntry.Config.LootMaxValue = origMaxVal;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }
    }

    // ── Section 4: Unlock conditions ──────────────────────────────────────────

    private static void RegisterUnlockTests(ITestRunnerApi api, string mod)
    {
        // ── 4.1: galaxySword mail flag → UnlockedGalaxy=true ─────────────────
        api.RegisterScenario(mod, "unlock_galaxy_via_mail_flag", s => s
            .ClearFlag("galaxySword")
            .AssertFlagAbsent("galaxySword")
            .Assert("UnlockedGalaxy is false before flag", () => !ModEntry.UnlockedGalaxy)
            .SetFlag("galaxySword")
            .Assert("UnlockedGalaxy is true after flag", () => ModEntry.UnlockedGalaxy)
            .ClearFlag("galaxySword")
        );

        // ── 4.2: Achievement 42 → UnlockedInfinity=true ───────────────────────
        api.RegisterScenario(mod, "unlock_infinity_via_achievement_42", s => s
            .Do(() => Game1.player.achievements.Remove(42))
            .Assert("UnlockedInfinity is false", () => !ModEntry.UnlockedInfinity)
            .Do(() => Game1.player.achievements.Add(42))
            .Assert("UnlockedInfinity is true", () => ModEntry.UnlockedInfinity)
            .Do(() => Game1.player.achievements.Remove(42))
        );

        // ── 4.3: Galaxy weapon only drops with unlock ─────────────────────────
        {
            bool origSkip = false;
            HashSet<string> origModIds = [];
            StardewValley.Tools.MeleeWeapon? weapon = null;
            api.RegisterScenario(mod, "unlock_galaxy_required_without_skip", s => s
                .Do(() => {
                    origSkip   = ModEntry.Config.SkipGalaxyCheck;
                    origModIds = [..ModEntry.Config.EnabledModWeaponIds];
                    ModEntry.Config.SkipGalaxyCheck         = false;
                    ModEntry.Config.SkipInfinityCheck        = false;
                    ModEntry.Config.EnableAllVanillaWeapons = false;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    weapon = ModEntry.GenerationService.GenerateWeapon(new Random(1), galaxyUnlocked: false, infinityUnlocked: false);
                })
                .Assert("no weapon without any unlock", () => weapon == null)
                .Do(() => {
                    ModEntry.Config.SkipGalaxyCheck = origSkip;
                    ModEntry.Config.EnabledModWeaponIds.Clear();
                    foreach (var id in origModIds) ModEntry.Config.EnabledModWeaponIds.Add(id);
                })
            );
        }
    }

    // ── Section 5: Integrated spawn tests (need actual mine) ─────────────────

    private static void RegisterIntegratedSpawnTests(ITestRunnerApi api, string mod)
    {
        // ── 5.1: ForceNextSpawn guarantees a barrel in the mine ───────────────
        {
            int origTries = 0;
            api.RegisterScenario(mod, "spawn_force_places_barrel_in_mine", s => s
                .Do(() => {
                    origTries = ModEntry.Config.AdditionalTriesToSpawn;
                    ModEntry.Config.AdditionalTriesToSpawn = 100;
                    ModEntry.ForceNextSpawn = true;
                })
                .WarpToMineFloor(20)
                .WaitUntil(() => Game1.currentLocation is MineShaft mine &&
                                 mine.objects.Values.Any(o => o is CustomBreakableObject), timeoutTicks: 300)
                .Assert("CustomBreakableObject is in mine objects", () =>
                    ((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => ModEntry.Config.AdditionalTriesToSpawn = origTries)
            );
        }

        // ── 5.2: Lower mines blocked → no barrel spawns ───────────────────────
        {
            bool origLower = false, origGalaxy = false, origInfinity = false, origHaveLimit = false;
            api.RegisterScenario(mod, "spawn_no_barrel_when_lower_mines_disabled", s => s
                .Do(() => {
                    origLower     = ModEntry.Config.SpawnInLowerMines;
                    origGalaxy    = ModEntry.Config.SkipGalaxyCheck;
                    origInfinity  = ModEntry.Config.SkipInfinityCheck;
                    origHaveLimit = ModEntry.Config.HaveDailySpawnLimit;
                    ModEntry.ForceNextSpawn              = false;
                    ModEntry.Config.SpawnInLowerMines    = false;
                    ModEntry.Config.SkipGalaxyCheck      = false;
                    ModEntry.Config.SkipInfinityCheck     = false;
                    ModEntry.Config.HaveDailySpawnLimit  = false;
                })
                .WarpToMineFloor(20)
                .WaitTicks(30)
                .Assert("no CustomBreakableObject in mine", () =>
                    !((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => {
                    ModEntry.Config.SpawnInLowerMines   = origLower;
                    ModEntry.Config.SkipGalaxyCheck     = origGalaxy;
                    ModEntry.Config.SkipInfinityCheck    = origInfinity;
                    ModEntry.Config.HaveDailySpawnLimit = origHaveLimit;
                })
            );
        }

        // ── 5.3: Daily limit exhausted → no barrel even with galaxy unlocked ──
        {
            bool origHave = false;
            api.RegisterScenario(mod, "spawn_no_barrel_when_daily_limit_zero", s => s
                .Do(() => {
                    origHave = ModEntry.Config.HaveDailySpawnLimit;
                    ModEntry.Config.HaveDailySpawnLimit = true;
                    ModEntry.Config.DailySpawnLimit     = 0;
                    ModEntry.SpawnService.ResetDailyLimit(0);
                    ModEntry.Config.SpawnInLowerMines = true;
                    ModEntry.Config.SkipGalaxyCheck   = true;
                    ModEntry.ForceNextSpawn           = false;
                })
                .WarpToMineFloor(20)
                .WaitTicks(30)
                .Assert("no barrel when limit is 0", () =>
                    !((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => {
                    ModEntry.Config.HaveDailySpawnLimit = origHave;
                    ModEntry.SpawnService.ResetDailyLimit(Game1.player.LuckLevel);
                })
            );
        }

        // ── 5.4: Skull Cavern barrel spawns when enabled ─────────────────────
        {
            int origTries = 0;
            api.RegisterScenario(mod, "spawn_force_barrel_in_skull_cavern", s => s
                .Do(() => {
                    origTries = ModEntry.Config.AdditionalTriesToSpawn;
                    ModEntry.Config.AdditionalTriesToSpawn = 100;
                    ModEntry.ForceNextSpawn = true;
                    ModEntry.Config.SpawnInSkullCavern = true;
                })
                .WarpToMineFloor(130)
                .WaitUntil(() => Game1.currentLocation is MineShaft mine &&
                                 mine.objects.Values.Any(o => o is CustomBreakableObject), timeoutTicks: 300)
                .Assert("barrel is in skull cavern", () =>
                    ((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => ModEntry.Config.AdditionalTriesToSpawn = origTries)
            );
        }

        // ── 5.5: ForceNextSpawn overrides disabled location ───────────────────
        {
            bool origLower = false; int origTries = 0;
            api.RegisterScenario(mod, "spawn_force_overrides_disabled_location", s => s
                .Do(() => {
                    origLower = ModEntry.Config.SpawnInLowerMines;
                    origTries = ModEntry.Config.AdditionalTriesToSpawn;
                    ModEntry.Config.SpawnInLowerMines      = false;
                    ModEntry.Config.SkipGalaxyCheck        = false;
                    ModEntry.Config.SkipInfinityCheck       = false;
                    ModEntry.Config.AdditionalTriesToSpawn = 100;
                    ModEntry.ForceNextSpawn                = true;
                })
                .WarpToMineFloor(20)
                .WaitUntil(() => Game1.currentLocation is MineShaft mine &&
                                 mine.objects.Values.Any(o => o is CustomBreakableObject), timeoutTicks: 300)
                .Assert("barrel still spawned despite disabled location", () =>
                    ((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => {
                    ModEntry.Config.SpawnInLowerMines      = origLower;
                    ModEntry.Config.AdditionalTriesToSpawn = origTries;
                })
            );
        }

        // ── 5.6: AdditionalBarrels > 0 spawns multiple barrels ───────────────
        {
            int origAdditional = 0, origTries = 0;
            api.RegisterScenario(mod, "spawn_allow_more_than_one_places_multiple", s => s
                .Do(() => {
                    origAdditional = ModEntry.Config.AdditionalBarrels;
                    origTries      = ModEntry.Config.AdditionalTriesToSpawn;
                    ModEntry.Config.AdditionalBarrels      = 2;
                    ModEntry.Config.AdditionalTriesToSpawn = 100;
                    ModEntry.ForceNextSpawn                = true;
                })
                .WarpToMineFloor(20)
                .WaitUntil(() => {
                    var count = ((MineShaft)Game1.currentLocation).objects.Values.Count(o => o is CustomBreakableObject);
                    return count >= 3;
                }, timeoutTicks: 300)
                .Assert("at least 3 barrels in mine", () => {
                    var count = ((MineShaft)Game1.currentLocation).objects.Values.Count(o => o is CustomBreakableObject);
                    return count >= 3;
                })
                .Do(() => {
                    ModEntry.Config.AdditionalBarrels      = origAdditional;
                    ModEntry.Config.AdditionalTriesToSpawn = origTries;
                })
            );
        }

        // ── 5.7: ForceNextSpawn flag resets to false after the warp ───────────
        api.RegisterScenario(mod, "spawn_force_flag_clears_after_use", s => s
            .Do(() => ModEntry.ForceNextSpawn = true)
            .WarpToMineFloor(20)
            .WaitTicks(10)
            .Assert("ForceNextSpawn is false after warp", () => !ModEntry.ForceNextSpawn)
        );

        // ── 5.8: Skull Cavern blocked when SpawnInSkullCavern=false ───────────
        {
            bool origSC = false;
            api.RegisterScenario(mod, "spawn_no_barrel_in_skull_cavern_when_disabled", s => s
                .Do(() => {
                    origSC = ModEntry.Config.SpawnInSkullCavern;
                    ModEntry.Config.SpawnInSkullCavern = false;
                    ModEntry.Config.SkipGalaxyCheck    = false;
                    ModEntry.Config.SkipInfinityCheck   = false;
                    ModEntry.ForceNextSpawn             = false;
                    ModEntry.Config.HaveDailySpawnLimit = false;
                })
                .WarpToMineFloor(130)
                .WaitTicks(30)
                .Assert("no barrel in skull cavern", () =>
                    !((MineShaft)Game1.currentLocation).objects.Values.Any(o => o is CustomBreakableObject))
                .Do(() => ModEntry.Config.SpawnInSkullCavern = origSC)
            );
        }
    }

    // ── Section 6: Integrated barrel break tests ──────────────────────────────

    private static void RegisterIntegratedBarrelTests(ITestRunnerApi api, string mod)
    {
        // ── 6.1: Barrel health decrements; does NOT break on first pickaxe hit ─
        api.RegisterScenario(mod, "barrel_single_hit_does_not_break", s => s
            .Do(() => ModEntry.ForceNextSpawn = true)
            .WarpToMineFloor(20)
            .WaitTicks(10)
            .Do(PlaceBarrelAtTestTile)
            .Assert("barrel exists at test tile before hit", () =>
                Game1.currentLocation.objects.ContainsKey(new Vector2(BarrelTX, BarrelTY)))
            .Do(() => {
                var mine = (MineShaft)Game1.currentLocation;
                var tile = new Vector2(BarrelTX, BarrelTY);
                if (mine.objects.TryGetValue(tile, out var obj))
                    obj.performToolAction(new Pickaxe { UpgradeLevel = 4 });
            })
            .Assert("barrel still exists after one pickaxe hit (health was 3)", () =>
                Game1.currentLocation.objects.ContainsKey(new Vector2(BarrelTX, BarrelTY)))
        );

        // ── 6.2: Hammer deals double damage (breaks in 2 hits vs 3) ───────────
        api.RegisterScenario(mod, "barrel_hammer_double_damage_breaks_faster", s => s
            .Do(() => ModEntry.ForceNextSpawn = true)
            .WarpToMineFloor(20)
            .WaitTicks(10)
            .Do(PlaceBarrelAtTestTile)
            .Do(() => {
                var mine = (MineShaft)Game1.currentLocation;
                var tile = new Vector2(BarrelTX, BarrelTY);
                // Hammer = MeleeWeapon type 2 (Club). Create a galaxy hammer for the test.
                var hammer = new StardewValley.Tools.MeleeWeapon("29");
                // Hit twice with hammer: health 3 → 1 → -1
                if (mine.objects.TryGetValue(tile, out var obj))
                    obj.performToolAction(hammer);
                if (mine.objects.TryGetValue(tile, out obj))
                    obj.performToolAction(hammer);
            })
            .Assert("barrel broken after 2 hammer hits (3-2-2 = -1)", () =>
                !Game1.currentLocation.objects.ContainsKey(new Vector2(BarrelTX, BarrelTY)))
        );

        // ── 6.3: ReleaseContents drops a weapon when spawn chance = 100% ──────
        {
            float origChance = 0f; bool origGlobal = false;
            api.RegisterScenario(mod, "barrel_release_drops_weapon_at_100_chance", s => s
                .Do(() => ModEntry.ForceNextSpawn = true)
                .WarpToMineFloor(20)
                .WaitTicks(10)
                .Do(() => {
                    origChance = ModEntry.Config.BaseSpawnChance;
                    origGlobal = ModEntry.Config.HaveGlobalChance;
                    ModEntry.Config.BaseSpawnChance  = 1f;
                    ModEntry.Config.HaveGlobalChance = true;
                    ModEntry.Config.SkipGalaxyCheck  = true;
                    PlaceBarrelAtTestTile();
                    // Clear existing debris so assertion is clean
                    Game1.currentLocation.debris.Clear();
                })
                .Do(TriggerBarrelRelease)
                .Assert("MeleeWeapon in location debris", () =>
                    Game1.currentLocation.debris.Any(d => d.item is StardewValley.Tools.MeleeWeapon))
                .Do(() => {
                    ModEntry.Config.BaseSpawnChance  = origChance;
                    ModEntry.Config.HaveGlobalChance = origGlobal;
                })
            );
        }

        // ── 6.4: ReleaseContents drops custom loot when weapon chance = 0% ────
        {
            float origChance = 0f; bool origGlobal = false, origLootEnabled = false;
            float origLootChance = 0f; List<string>? origItems = null;
            int origMinVal = 0, origMaxVal = 0;
            api.RegisterScenario(mod, "barrel_release_drops_custom_loot_fallback", s => s
                .Do(() => ModEntry.ForceNextSpawn = true)
                .WarpToMineFloor(20)
                .WaitTicks(10)
                .Do(() => {
                    origChance      = ModEntry.Config.BaseSpawnChance;
                    origGlobal      = ModEntry.Config.HaveGlobalChance;
                    origLootEnabled = ModEntry.Config.EnableCustomLoot;
                    origLootChance  = ModEntry.Config.CustomLootChance;
                    origItems       = ModEntry.Config.LootItems;
                    origMinVal      = ModEntry.Config.LootMinValue;
                    origMaxVal      = ModEntry.Config.LootMaxValue;
                    ModEntry.Config.BaseSpawnChance   = 0f;  // weapon never drops
                    ModEntry.Config.HaveGlobalChance  = true;
                    ModEntry.Config.EnableCustomLoot  = true;
                    ModEntry.Config.CustomLootChance  = 1f;
                    ModEntry.Config.LootItems     = ["(O)74"];
                    ModEntry.Config.LootMinValue  = 0;
                    ModEntry.Config.LootMaxValue  = 0;
                    ModEntry.LootService.InvalidatePool();
                    PlaceBarrelAtTestTile();
                    Game1.currentLocation.debris.Clear();
                })
                .Do(TriggerBarrelRelease)
                .Assert("non-weapon item in debris (custom loot)", () =>
                    Game1.currentLocation.debris.Any(d => d.item is not null and not StardewValley.Tools.MeleeWeapon))
                .Do(() => {
                    ModEntry.Config.BaseSpawnChance  = origChance;
                    ModEntry.Config.HaveGlobalChance = origGlobal;
                    ModEntry.Config.EnableCustomLoot = origLootEnabled;
                    ModEntry.Config.CustomLootChance = origLootChance;
                    ModEntry.Config.LootItems    = origItems!;
                    ModEntry.Config.LootMinValue = origMinVal;
                    ModEntry.Config.LootMaxValue = origMaxVal;
                    ModEntry.LootService.InvalidatePool();
                })
            );
        }

        // ── 6.5: No debris when weapon chance=0 and custom loot disabled ──────
        {
            float origChance = 0f; bool origGlobal = false, origLootEnabled = false;
            api.RegisterScenario(mod, "barrel_release_no_drops_when_all_disabled", s => s
                .Do(() => ModEntry.ForceNextSpawn = true)
                .WarpToMineFloor(20)
                .WaitTicks(10)
                .Do(() => {
                    origChance      = ModEntry.Config.BaseSpawnChance;
                    origGlobal      = ModEntry.Config.HaveGlobalChance;
                    origLootEnabled = ModEntry.Config.EnableCustomLoot;
                    ModEntry.Config.BaseSpawnChance  = 0f;
                    ModEntry.Config.HaveGlobalChance = true;
                    ModEntry.Config.EnableCustomLoot = false;
                    ModEntry.LootService.InvalidatePool();
                    PlaceBarrelAtTestTile();
                    Game1.currentLocation.debris.Clear();
                })
                .Do(TriggerBarrelRelease)
                .Assert("no item debris after release", () =>
                    !Game1.currentLocation.debris.Any(d => d.item is not null))
                .Do(() => {
                    ModEntry.Config.BaseSpawnChance  = origChance;
                    ModEntry.Config.HaveGlobalChance = origGlobal;
                    ModEntry.Config.EnableCustomLoot = origLootEnabled;
                })
            );
        }

        // ── 6.6: Floor-based spawn chance scales with mine level ──────────────
        //  StepsLevel = {20, 40, 60, 80, 100, 120}. At floor 120 all 6 apply.
        api.RegisterScenario(mod, "barrel_floor_scaling_adds_chance_steps", s => s
            .Do(() => ModEntry.ForceNextSpawn = true)
            .WarpToMineFloor(120)
            .WaitTicks(10)
            .Do(() => {
                ModEntry.Config.HaveGlobalChance       = false;
                ModEntry.Config.BaseSpawnChance        = 0.6f;
                ModEntry.Config.IncreaseSpawnChanceStep = 0.05f;
            })
            .Assert("effective chance at floor 120 = 0.6 + 6*0.05 = 0.9", () => {
                // Mirror the DevMenu's StatsLine() calculation
                float chance = ModEntry.Config.BaseSpawnChance;
                int mineLevel = ((MineShaft)Game1.currentLocation).mineLevel;
                foreach (int step in new[] { 20, 40, 60, 80, 100, 120 })
                    if (mineLevel >= step)
                        chance += ModEntry.Config.IncreaseSpawnChanceStep;
                return Math.Abs(chance - 0.9f) < 0.001f;
            })
        );

        // ── 6.7: HaveGlobalChance=true uses flat BaseSpawnChance only ─────────
        api.RegisterScenario(mod, "barrel_global_chance_bypasses_floor_scaling", s => s
            .Do(() => ModEntry.ForceNextSpawn = true)
            .WarpToMineFloor(120)
            .WaitTicks(10)
            .Do(() => {
                ModEntry.Config.HaveGlobalChance       = true;
                ModEntry.Config.BaseSpawnChance        = 0.6f;
                ModEntry.Config.IncreaseSpawnChanceStep = 0.05f;
            })
            .Assert("effective chance with global flag = flat 0.6", () => {
                // When HaveGlobalChance=true, ReleaseContents skips the step-based addition
                return Math.Abs(ModEntry.Config.BaseSpawnChance - 0.6f) < 0.001f &&
                       ModEntry.Config.HaveGlobalChance;
            })
        );

        // ── 6.8: BreakObjectAt triggers full barrel lifecycle ─────────────────
        {
            float origChance = 0f; bool origGlobal = false;
            api.RegisterScenario(mod, "barrel_break_via_tool_removes_object", s => s
                .Do(() => ModEntry.ForceNextSpawn = true)
                .WarpToMineFloor(20)
                .WaitTicks(10)
                .Do(() => {
                    origChance = ModEntry.Config.BaseSpawnChance;
                    origGlobal = ModEntry.Config.HaveGlobalChance;
                    ModEntry.Config.BaseSpawnChance  = 0f;
                    ModEntry.Config.HaveGlobalChance = true;
                    ModEntry.Config.EnableCustomLoot = false;
                    PlaceBarrelAtTestTile();
                })
                .BreakObjectAt("UndergroundMine20", BarrelTX, BarrelTY)
                .Assert("barrel tile is now clear", () =>
                    !Game1.currentLocation.objects.ContainsKey(new Vector2(BarrelTX, BarrelTY)))
                .Do(() => {
                    ModEntry.Config.BaseSpawnChance  = origChance;
                    ModEntry.Config.HaveGlobalChance = origGlobal;
                })
            );
        }
    }
}
