using EnchantedGalaxyWeapons.Config;
using StardewModdingAPI;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class SpawnMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            // ── Spawn Settings ────────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.spawn-settings"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.main-daily-limit"),
                tooltip: () => helper.Translation.Get("menu.main-daily-limit-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.HaveDailySpawnLimit,
                setValue: v => config.HaveDailySpawnLimit = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.main-daily-limit-amount"),
                tooltip: () => helper.Translation.Get("menu.main-daily-limit-amount-tooltip-p1") +
                               helper.Translation.Get("menu.main-daily-limit-amount-tooltip-p2") +
                               helper.Translation.Get("menu.default", new { defaultValue = 5 }),
                getValue: () => config.DailySpawnLimit,
                setValue: v => config.DailySpawnLimit = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.main-barrels-amount"),
                tooltip: () => helper.Translation.Get("menu.main-barrels-amount-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = 0 }),
                getValue: () => config.AdditionalBarrels,
                setValue: v => config.AdditionalBarrels = v,
                min: 0, max: 40);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.main-additional-attempts"),
                tooltip: () => helper.Translation.Get("menu.main-additional-attempts-tooltip-p1") +
                               helper.Translation.Get("menu.main-additional-attempts-tooltip-p2") +
                               helper.Translation.Get("menu.default", new { defaultValue = 0 }),
                getValue: () => config.AdditionalTriesToSpawn,
                setValue: v => config.AdditionalTriesToSpawn = v,
                min: 0, max: 50);

            // ── Drop Chance ───────────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.spawn-drop-chance"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.main-global-chance"),
                tooltip: () => helper.Translation.Get("menu.main-global-chance-tooltip-p1") +
                               helper.Translation.Get("menu.main-global-chance-tooltip-p2") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.HaveGlobalChance,
                setValue: v => config.HaveGlobalChance = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.main-base-chance"),
                tooltip: () => helper.Translation.Get("menu.default", new { defaultValue = "60%" }),
                getValue: () => config.BaseSpawnChance,
                setValue: v => config.BaseSpawnChance = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.05f);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.main-increment-chance"),
                tooltip: () => helper.Translation.Get("menu.main-increment-chance-tooltip-p1") +
                               helper.Translation.Get("menu.main-increment-chance-tooltip-p2") +
                               helper.Translation.Get("menu.default", new { defaultValue = "5%" }),
                getValue: () => config.IncreaseSpawnChanceStep,
                setValue: v => config.IncreaseSpawnChanceStep = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.05f);

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.main-dangerous-chance"));
        }
    }
}
