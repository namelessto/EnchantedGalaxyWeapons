using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Menu;
using EnchantedGalaxyWeapons.Services;
using EnchantedGalaxyWeapons.Tests;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using TestRunner.Api;

namespace EnchantedGalaxyWeapons
{
    internal sealed class ModEntry : Mod
    {
        private ModConfig _config = new();
        private IGenericModConfigMenuApi? _configMenu;
        private bool _configMenuRegistered;

        public static ModConfig Config { get; private set; } = new();
        public static bool UnlockedGalaxy => Game1.player?.mailReceived?.Contains("galaxySword") ?? false;
        public static bool UnlockedInfinity => Game1.player?.achievements?.Contains(42) ?? false;
        public static WeaponSpawnService SpawnService { get; private set; } = null!;
        public static WeaponGenerationService GenerationService { get; private set; } = null!;
        public static CustomLootService LootService { get; private set; } = null!;
        public static bool ForceNextSpawn { get; set; }

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            Config = _config;

            SpawnService = new WeaponSpawnService(helper, _config);
            GenerationService = new WeaponGenerationService(_config);
            LootService = new CustomLootService(_config);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            //helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            _configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_configMenu is not null)
                Helper.Events.GameLoop.UpdateTicked += OnFirstTick;

            var testRunner = Helper.ModRegistry.GetApi<ITestRunnerApi>("namelessto.TestRunner");
            if (testRunner is not null)
                EGWTests.Register(testRunner);
        }

        private void OnFirstTick(object? sender, UpdateTickedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= OnFirstTick;
            BuildConfigMenu();
            Helper.WriteConfig(_config);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (_configMenu is null) return;
            int before = _config.EnabledModWeaponIds.Count;
            BuildConfigMenu();
            if (_config.EnabledModWeaponIds.Count != before)
                Helper.WriteConfig(_config);
        }

        private void BuildConfigMenu()
        {
            if (_configMenuRegistered)
                _configMenu!.Unregister(ModManifest);

            _configMenu!.Register(
                mod: ModManifest,
                reset: () =>
                {
                    var defaults = new ModConfig();
                    foreach (var prop in typeof(ModConfig).GetProperties()
                        .Where(p => p.CanWrite
                               && p.Name != nameof(ModConfig.EnabledModWeaponIds)
                               && p.Name != nameof(ModConfig.SeenModWeaponPrefixes)))
                        prop.SetValue(_config, prop.GetValue(defaults));

                    // Restore mod weapons to default: all detected mods enabled.
                    var groups = WeaponsMenuPage.BuildPrefixGroups(Helper);
                    _config.SeenModWeaponPrefixes = new HashSet<string>(groups.Keys, StringComparer.OrdinalIgnoreCase);
                    _config.EnabledModWeaponIds = new HashSet<string>(groups.Keys, StringComparer.OrdinalIgnoreCase);

                    LootService.InvalidatePool();
                },
                save: () => { Helper.WriteConfig(_config); LootService.InvalidatePool(); });

            _configMenuRegistered = true;
            ModMenu.BuildMenu(Helper, ModManifest, _configMenu, _config, Monitor);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            SpawnService.ResetDailyLimit(Game1.player.LuckLevel);
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft mine)
                SpawnService.TrySpawnBarrel(mine);
        }

        //private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        //{
        //    if (!Context.IsPlayerFree) return;
        //    if (_config.DevMenuKey.JustPressed())
        //        Game1.activeClickableMenu = new DevMenu();
        //}
    }
}
