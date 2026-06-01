using EnchantedGalaxyWeapons.Config;
using StardewModdingAPI;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class CustomLootMenuPage
    {
        // Shown as checkboxes in GMCM for quick manual additions.
        // Any item ID (including modded) can be added to LootItems in config.json directly.
        private static readonly (string Id, string Name)[] PredefinedItems =
        {
            ("(O)749", "Omni Geode"),
            ("(O)535", "Geode"),
            ("(O)536", "Frozen Geode"),
            ("(O)537", "Magma Geode"),
            ("(O)386", "Iridium Ore"),
            ("(O)337", "Iridium Bar"),
            ("(O)384", "Gold Ore"),
            ("(O)336", "Gold Bar"),
            ("(O)380", "Iron Ore"),
            ("(O)335", "Iron Bar"),
            ("(O)378", "Copper Ore"),
            ("(O)334", "Copper Bar"),
            ("(O)74",  "Prismatic Shard"),
            ("(O)72",  "Diamond"),
            ("(O)64",  "Ruby"),
            ("(O)60",  "Emerald"),
            ("(O)62",  "Aquamarine"),
            ("(O)66",  "Amethyst"),
            ("(O)68",  "Topaz"),
            ("(O)70",  "Jade"),
            ("(O)82",  "Fire Quartz"),
            ("(O)84",  "Frozen Tear"),
            ("(O)86",  "Earth Crystal"),
            ("(O)80",  "Quartz"),
        };

        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            configMenu.AddPage(manifest, "loot",
                () => helper.Translation.Get("menu.loot-title"));

            // ── Settings ──────────────────────────────────────────────────────
            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.loot-enable"),
                tooltip: () => helper.Translation.Get("menu.loot-enable-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.EnableCustomLoot,
                setValue: v => config.EnableCustomLoot = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.loot-chance"),
                tooltip: () => helper.Translation.Get("menu.loot-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "50%" }),
                getValue: () => config.CustomLootChance,
                setValue: v => config.CustomLootChance = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.05f);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.loot-min-stack"),
                tooltip: () => helper.Translation.Get("menu.loot-stack-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 1 }),
                getValue: () => config.LootMinStack,
                setValue: v => config.LootMinStack = v,
                min: 1, max: 25);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.loot-max-stack"),
                tooltip: () => helper.Translation.Get("menu.loot-stack-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 5 }),
                getValue: () => config.LootMaxStack,
                setValue: v => config.LootMaxStack = v,
                min: 1, max: 25);

            // ── Value-Based Pool ──────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.loot-value-title"));

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.loot-value-info"));

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.loot-min-value"),
                tooltip: () => helper.Translation.Get("menu.loot-min-value-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 200 }),
                getValue: () => config.LootMinValue,
                setValue: v => config.LootMinValue = v,
                min: 0, max: 10000, interval: 50);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.loot-max-value"),
                tooltip: () => helper.Translation.Get("menu.loot-max-value-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 0 }),
                getValue: () => config.LootMaxValue,
                setValue: v => config.LootMaxValue = v,
                min: 0, max: 10000, interval: 50);

            // ── Manual Items ──────────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.loot-items-title"));

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.loot-custom-info"));

            foreach (var (id, name) in PredefinedItems)
            {
                string capturedId   = id;
                string capturedName = name;
                configMenu.AddBoolOption(manifest,
                    name:     () => capturedName,
                    tooltip:  () => capturedId,
                    getValue: () => config.LootItems.Contains(capturedId),
                    setValue: v =>
                    {
                        if (v) { if (!config.LootItems.Contains(capturedId)) config.LootItems.Add(capturedId); }
                        else   config.LootItems.Remove(capturedId);
                    });
            }
        }
    }
}
