using EnchantedGalaxyWeapons.Config;
using StardewModdingAPI;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class ModMenu
    {
        internal static string ToKey(Enum value)
        {
            string name = value.ToString();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                    sb.Append('-');
                sb.Append(char.ToLower(name[i]));
            }
            return sb.ToString();
        }

        public static void BuildMenu(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config, IMonitor monitor)
        {
            SpawnMenuPage.Build(helper, manifest, configMenu, config);

            configMenu.AddPageLink(manifest, "weapons",
                () => helper.Translation.Get("menu.weapons-title"));
            configMenu.AddPageLink(manifest, "enchantments",
                () => helper.Translation.Get("menu.enchantments-title"));
            configMenu.AddPageLink(manifest, "spawnLocations",
                () => helper.Translation.Get("menu.locations-title"));
            configMenu.AddPageLink(manifest, "loot",
                () => helper.Translation.Get("menu.loot-title"));

            WeaponsMenuPage.Build(helper, manifest, configMenu, config, monitor);
            EnchantmentsMenuPage.Build(helper, manifest, configMenu, config);
            SpawnLocationsMenuPage.Build(helper, manifest, configMenu, config);
            CustomLootMenuPage.Build(helper, manifest, configMenu, config);
        }
    }
}
