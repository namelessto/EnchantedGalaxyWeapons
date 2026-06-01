using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
using StardewModdingAPI;
using System;

namespace EnchantedGalaxyWeapons.Menu
{
    internal static class EnchantmentsMenuPage
    {
        public static void Build(IModHelper helper, IManifest manifest, IGenericModConfigMenuApi configMenu, ModConfig config)
        {
            configMenu.AddPage(manifest, "enchantments",
                () => helper.Translation.Get("menu.enchantments"));

            // ── Regular Enchantments ──────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.enchantments-regular-options"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-guarantee-regular"),
                tooltip: () => helper.Translation.Get("menu.enchantments-guarantee-regular-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.ForceHaveEnchantment,
                setValue: v => config.ForceHaveEnchantment = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-regular-chance"),
                tooltip: () => helper.Translation.Get("menu.enchantments-regular-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "15%" }),
                getValue: () => config.ChanceForEnchantment,
                setValue: v => config.ChanceForEnchantment = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.01f);

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-select"));

            foreach (EnchantmentType t in Enum.GetValues<EnchantmentType>())
            {
                var captured = t;
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get($"menu.enchantments-{ModMenu.ToKey(captured)}"),
                    getValue: () => config.AllowedEnchantments.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedEnchantments[captured] = v);
            }

            // ── Innate Enchantments ───────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.enchantments-innate-options"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-innate-keep"),
                tooltip: () => helper.Translation.Get("menu.enchantments-innate-keep-tooltip"),
                getValue: () => config.KeepVanilla,
                setValue: v => config.KeepVanilla = v);

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-guarantee-innate"),
                tooltip: () => helper.Translation.Get("menu.enchantments-guarantee-innate-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.ForceInnateEnchantment,
                setValue: v => config.ForceInnateEnchantment = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-innate-chance"),
                tooltip: () => helper.Translation.Get("menu.enchantments-innate-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "50%" }),
                getValue: () => config.ChanceForInnate,
                setValue: v => config.ChanceForInnate = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.01f);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-min-innate"),
                getValue: () => config.MinInnateEnchantments,
                setValue: v => config.MinInnateEnchantments = v,
                min: 1, max: 8);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-max-innate"),
                getValue: () => config.MaxInnateEnchantments,
                setValue: v => config.MaxInnateEnchantments = v,
                min: 1, max: 8);

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-innate-limit-p1") +
                            helper.Translation.Get("menu.enchantments-innate-limit-p2"));

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-select"));

            foreach (InnateType t in Enum.GetValues<InnateType>())
            {
                var captured = t;
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get($"menu.enchantments-{ModMenu.ToKey(captured)}"),
                    getValue: () => config.AllowedStats.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedStats[captured] = v);
            }

            // ── Gem Enchantments ──────────────────────────────────────────────
            configMenu.AddSectionTitle(manifest,
                text: () => helper.Translation.Get("menu.enchantments-gem-options"));

            configMenu.AddBoolOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-guarantee-gem"),
                tooltip: () => helper.Translation.Get("menu.enchantments-guarantee-gem-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = helper.Translation.Get("menu.false") }),
                getValue: () => config.ForceGemEnchantment,
                setValue: v => config.ForceGemEnchantment = v);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-gem-chance"),
                tooltip: () => helper.Translation.Get("menu.enchantments-gem-chance-tooltip") +
                               helper.Translation.Get("menu.default", new { defaultValue = "10%" }),
                getValue: () => config.ChanceForGemEnchantment,
                setValue: v => config.ChanceForGemEnchantment = v,
                min: 0f, max: 1f,
                formatValue: v => $"{Math.Round(v * 100)}%",
                interval: 0.01f);

            configMenu.AddTextOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-gem-mode"),
                tooltip: () => helper.Translation.Get("menu.enchantments-gem-mode-tooltip"),
                getValue: () => config.GemMode.ToString(),
                setValue: v => config.GemMode = Enum.Parse<GemApplicationMode>(v),
                allowedValues: Enum.GetNames<GemApplicationMode>(),
                formatAllowedValue: v => helper.Translation.Get($"menu.enchantments-gem-mode-{v.ToLower()}"));

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-min-gem-level"),
                tooltip: () => helper.Translation.Get("menu.enchantments-gem-level-tooltip"),
                getValue: () => config.MinGemLevel,
                setValue: v => config.MinGemLevel = v,
                min: 1, max: 3);

            configMenu.AddNumberOption(manifest,
                name: () => helper.Translation.Get("menu.enchantments-max-gem-level"),
                tooltip: () => helper.Translation.Get("menu.enchantments-gem-level-tooltip"),
                getValue: () => config.MaxGemLevel,
                setValue: v => config.MaxGemLevel = v,
                min: 1, max: 3);

            configMenu.AddParagraph(manifest,
                text: () => helper.Translation.Get("menu.enchantments-gem-select"));

            foreach (GemEnchantmentType t in Enum.GetValues<GemEnchantmentType>())
            {
                var captured = t;
                configMenu.AddBoolOption(manifest,
                    name: () => helper.Translation.Get($"menu.enchantments-{ModMenu.ToKey(captured)}"),
                    getValue: () => config.AllowedGemEnchantments.GetValueOrDefault(captured, true),
                    setValue: v => config.AllowedGemEnchantments[captured] = v);
            }
        }
    }
}
