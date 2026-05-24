using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using StardewModdingAPI;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class EnchantmentsMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            configMenu.AddPage(manifest, "enchantments",
                () => helper.Translation.Get("menu.enchantments"));

            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.basic-options"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-guarantee-regular"),
                tooltip: () => helper.Translation.Get("menu.enchantments-guarantee-regular-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.ForceHaveEnchantment,
                setValue: v => config.ForceHaveEnchantment = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-guarantee-innate"),
                tooltip: () => helper.Translation.Get("menu.enchantments-guarantee-innate-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.ForceInnateEnchantment,
                setValue: v => config.ForceInnateEnchantment = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-regular-chance"),
                tooltip: () => helper.Translation.Get("menu.enchantments-regular-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "15%" }),
                getValue: () => config.ChanceForEnchantment,
                setValue: v => config.ChanceForEnchantment = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.01f);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-innate-chance"),
                tooltip: () => helper.Translation.Get("menu.enchantments-innate-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "50%" }),
                getValue: () => config.ChanceForInnate,
                setValue: v => config.ChanceForInnate = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.01f);

            // Regular enchantments
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.enchantments-regular-options"));
            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-select"));

            foreach (EnchantmentType t in Enum.GetValues<EnchantmentType>())
            {
                var captured = t;
                string key = $"menu.enchantments-{ModMenu.ToKey(captured)}";
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get(key),
                    getValue: () => config.AllowedEnchantments.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedEnchantments[captured] = v);
            }

            // Innate enchantments
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.enchantments-innate-options"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-innate-keep"),
                tooltip: () => helper.Translation.Get("menu.enchantments-innate-keep-tooltip"),
                getValue: () => config.KeepVanilla,
                setValue: v => config.KeepVanilla = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-min-innate"),
                getValue: () => config.MinInnateEnchantments,
                setValue: v => config.MinInnateEnchantments = v,
                min: 0, max: 8);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-max-innate"),
                getValue: () => config.MaxInnateEnchantments,
                setValue: v => config.MaxInnateEnchantments = v,
                min: 0, max: 8);

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-innate-limit-p1") +
                            helper.Translation.Get("menu.enchantments-innate-limit-p2"));

            foreach (InnateType t in Enum.GetValues<InnateType>())
            {
                var captured = t;
                string key = $"menu.enchantments-{ModMenu.ToKey(captured)}";
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get(key),
                    getValue: () => config.AllowedStats.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedStats[captured] = v);
            }
        }
    }
}
