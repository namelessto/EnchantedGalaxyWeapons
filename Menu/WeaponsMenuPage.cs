using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Weapons;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class WeaponsMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config, IMonitor monitor)
        {
            configMenu.AddPage(manifest, "weapons",
                () => helper.Translation.Get("menu.weapons"));

            // ── Unlock Requirements ───────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-unlock-requirements"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.weapons-skip-galaxy"),
                tooltip: () => helper.Translation.Get("menu.weapons-skip-galaxy-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.SkipGalaxyCheck,
                setValue: v => config.SkipGalaxyCheck = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.weapons-skip-infinity"),
                tooltip: () => helper.Translation.Get("menu.weapons-skip-infinity-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.SkipInfinityCheck,
                setValue: v => config.SkipInfinityCheck = v);

            // ── Weapon Pool ───────────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-pool"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.weapons-enable-vanilla"),
                tooltip: () => helper.Translation.Get("menu.weapons-enable-vanilla-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.EnableAllVanillaWeapons,
                setValue: v => config.EnableAllVanillaWeapons = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.weapons-min-level"),
                tooltip: () => helper.Translation.Get("menu.weapons-min-level-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 0 }),
                getValue: () => config.MinWeaponLevel,
                setValue: v => config.MinWeaponLevel = v,
                min: 0, max: 100);

            // ── Type Weights ──────────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-category-weights"));

            foreach (WeaponCategory cat in Enum.GetValues<WeaponCategory>())
            {
                var captured = cat;
                configMenu.AddNumberOption(manifest,
                    name: () => helper.Translation.Get($"menu.weapons-category-{ModMenu.ToKey(captured)}"),
                    tooltip: () => helper.Translation.Get("menu.weapons-category-weight-tooltip"),
                    getValue: () => config.WeaponCategoryWeights.GetValueOrDefault(captured, 1f),
                    setValue: v => config.WeaponCategoryWeights[captured] = v,
                    min: 0f, max: 3f, interval: 0.1f);
            }

            // ── Galaxy & Infinity Weapons ─────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-allow"));

            foreach (WeaponType t in Enum.GetValues<WeaponType>())
            {
                var captured = t;
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get($"menu.weapons-{ModMenu.ToKey(captured)}"),
                    getValue: () => config.AllowedWeapons.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedWeapons[captured] = v);
            }

            // ── Mod Integrations ──────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.integrations-title"));

            var prefixGroups = BuildPrefixGroups(helper);

            if (prefixGroups.Count == 0)
                monitor.Log("No mod weapons detected.", LogLevel.Debug);
            else
                monitor.Log($"Detected mod weapon groups: {string.Join(", ", prefixGroups.Select(g => $"{g.Value}"))}.", LogLevel.Debug);

            foreach (var stale in config.SeenModWeaponPrefixes
                         .Except(prefixGroups.Keys, StringComparer.OrdinalIgnoreCase).ToList())
            {
                config.SeenModWeaponPrefixes.Remove(stale);
                config.EnabledModWeaponIds.Remove(stale);
            }

            foreach (string prefix in prefixGroups.Keys)
                if (config.SeenModWeaponPrefixes.Add(prefix))
                    config.EnabledModWeaponIds.Add(prefix);

            if (prefixGroups.Count == 0)
            {
                configMenu.AddParagraph(manifest,
                    text: () => helper.Translation.Get("menu.integrations-none"));
            }
            else
            {
                foreach (var (prefix, label) in prefixGroups)
                {
                    string capturedPrefix = prefix;
                    string capturedLabel  = label;
                    configMenu.AddBoolOption(manifest,
                        name: () => capturedLabel,
                        tooltip: () => capturedPrefix,
                        getValue: () => config.EnabledModWeaponIds.Contains(capturedPrefix),
                        setValue: v => { if (v) config.EnabledModWeaponIds.Add(capturedPrefix); else config.EnabledModWeaponIds.Remove(capturedPrefix); });
                }
            }
        }

        internal static Dictionary<string, string> BuildPrefixGroups(IModHelper helper)
        {
            var groups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Data/Weapons is cached before mods run, so CP patches aren't in it yet.
            // Invalidate first to force a fresh load that runs all AssetRequested handlers.
            helper.GameContent.InvalidateCache("Data/Weapons");

            // Union content pipeline (catches CP patches) with Game1.weaponData
            // (catches C# mods that write directly to the static dict).
            var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string k in helper.GameContent.Load<Dictionary<string, WeaponData>>("Data/Weapons").Keys)
                    allKeys.Add(k);
            }
            catch { }
            if (Game1.weaponData != null)
                foreach (string k in Game1.weaponData.Keys)
                    allKeys.Add(k);

            if (allKeys.Count == 0) return groups;

            var modById = helper.ModRegistry.GetAll()
                .ToDictionary(m => m.Manifest.UniqueID, m => m.Manifest.Name, StringComparer.OrdinalIgnoreCase);

            foreach (string key in allKeys)
            {
                if (int.TryParse(key, out _)) continue;

                string? matchedId = modById.Keys
                    .Where(id => HasModPrefix(key, id))
                    .OrderByDescending(id => id.Length)
                    .FirstOrDefault();

                if (matchedId != null)
                    groups.TryAdd(matchedId, modById[matchedId]);
                else
                    groups.TryAdd(ExtractPrefix(key), ExtractPrefix(key));
            }

            return groups;
        }

        private static bool HasModPrefix(string weaponKey, string modId) =>
            WeaponKeyHelper.HasModPrefix(weaponKey, modId);

        private static string ExtractPrefix(string key)
        {
            for (int i = 0; i < key.Length; i++)
                if (!char.IsLetterOrDigit(key[i]))
                    return key[..i];
            return key;
        }
    }
}
