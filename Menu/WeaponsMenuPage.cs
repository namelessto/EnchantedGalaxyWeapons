using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using StardewModdingAPI;
using StardewValley;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class WeaponsMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            configMenu.AddPage(manifest, "weapons",
                () => helper.Translation.Get("menu.weapons"));

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.basic-options"));

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

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-category-weights"));

            foreach (WeaponCategory cat in Enum.GetValues<WeaponCategory>())
            {
                var captured = cat;
                string key = $"menu.weapons-category-{ModMenu.ToKey(captured)}";
                configMenu.AddNumberOption(manifest,
                    name: () => helper.Translation.Get(key),
                    tooltip: () => helper.Translation.Get("menu.weapons-category-weight-tooltip"),
                    getValue: () => config.WeaponCategoryWeights.GetValueOrDefault(captured, 1f),
                    setValue: v => config.WeaponCategoryWeights[captured] = v,
                    min: 0f,
                    max: 3f,
                    interval: 0.1f);
            }

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.weapons-allow"));

            foreach (WeaponType t in Enum.GetValues<WeaponType>())
            {
                var captured = t;
                string key = $"menu.weapons-{ModMenu.ToKey(captured)}";
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get(key),
                    getValue: () => config.AllowedWeapons.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedWeapons[captured] = v);
            }

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.integrations-title"));

            var prefixGroups = BuildPrefixGroups(helper);

            foreach (string prefix in prefixGroups.Keys)
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
                    string capturedLabel = label;
                    configMenu.AddBoolOption(manifest,
                        name: () => capturedLabel,
                        tooltip: () => capturedPrefix,
                        getValue: () => config.EnabledModWeaponIds.Contains(capturedPrefix),
                        setValue: v => { if (v) config.EnabledModWeaponIds.Add(capturedPrefix); else config.EnabledModWeaponIds.Remove(capturedPrefix); });
                }
            }
        }

        // Returns prefix → display label for all detected weapon mod groups.
        private static Dictionary<string, string> BuildPrefixGroups(IModHelper helper)
        {
            var groups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Game1.weaponData == null) return groups;

            var modById = helper.ModRegistry.GetAll()
                .ToDictionary(m => m.Manifest.UniqueID, m => m.Manifest.Name, StringComparer.OrdinalIgnoreCase);

            foreach (string key in Game1.weaponData.Keys)
            {
                if (int.TryParse(key, out _)) continue;

                // Try matching to a known mod ID (prefer longest match to avoid partial hits)
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
            weaponKey.StartsWith(modId, StringComparison.OrdinalIgnoreCase) &&
            (weaponKey.Length == modId.Length || !char.IsLetterOrDigit(weaponKey[modId.Length]));

        private static string ExtractPrefix(string key)
        {
            for (int i = 0; i < key.Length; i++)
                if (!char.IsLetterOrDigit(key[i]))
                    return key[..i];
            return key;
        }
    }
}
