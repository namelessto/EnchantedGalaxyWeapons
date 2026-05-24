using EnchantedGalaxyWeapons.Config;
using StardewModdingAPI;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class SpawnLocationsMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            configMenu.AddPage(manifest, "spawnLocations",
                () => helper.Translation.Get("menu.locations"));

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.locations-title"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.locations-lower-mines"),
                tooltip: () => helper.Translation.Get("menu.locations-lower-mines-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.SpawnInLowerMines,
                setValue: v => config.SpawnInLowerMines = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.locations-mid-mines"),
                tooltip: () => helper.Translation.Get("menu.locations-mid-mines-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.SpawnInMidMines,
                setValue: v => config.SpawnInMidMines = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.locations-upper-mines"),
                tooltip: () => helper.Translation.Get("menu.locations-upper-mines-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.SpawnInUpperMines,
                setValue: v => config.SpawnInUpperMines = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.locations-dangerous"),
                tooltip: () => helper.Translation.Get("menu.locations-dangerous-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.SpawnInDangerousMines,
                setValue: v => config.SpawnInDangerousMines = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.locations-skull-cavern"),
                tooltip: () => helper.Translation.Get("menu.locations-skull-cavern-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.true") }),
                getValue: () => config.SpawnInSkullCavern,
                setValue: v => config.SpawnInSkullCavern = v);
        }
    }
}
