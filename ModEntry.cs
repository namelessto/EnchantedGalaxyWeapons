using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Menu;
using EnchantedGalaxyWeapons.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

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

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            Config = _config;

            SpawnService = new WeaponSpawnService(helper, Monitor, _config);
            GenerationService = new WeaponGenerationService(_config);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            _configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_configMenu is null) return;
            BuildConfigMenu();
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
                reset: () => { _config = new ModConfig(); Config = _config; },
                save: () => Helper.WriteConfig(_config));

            _configMenuRegistered = true;
            ModMenu.BuildMenu(Helper, ModManifest, _configMenu, _config);
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
    }
}
