using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using xTile.Layers;

namespace EnchantedGalaxyWeapons
{
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/

        public static Mod ModInstance;
        public static ModConfig Config;

        public static bool UnlockedGalaxy;
        public static bool UnlockedInfinity;
        public static int MaxSpawnForDay = 0;

        private HUDMessage Message = new("");

        /*********
        ** Public methods
        *********/

        /// <summary> The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModEntry.ModInstance = this;
            Config = this.Helper.ReadConfig<ModConfig>();
            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.DailyChecks;
            helper.Events.Player.Warped += this.CheckIfMine;
        }

        /*********
        ** Private methods
        *********/
        private void UpdateValuesFromConfig()
        {
            this.Helper.WriteConfig(Config);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => UpdateValuesFromConfig()
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Daily Limit",
                tooltip: () => "Whether to limit the number of barrels that can spawn per day. Disable to ignore the limit.\nDefault: true",
                getValue: () => Config.HaveDailySpawnLimit,
                setValue: value => Config.HaveDailySpawnLimit = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Daily Spawn Limit",
                tooltip: () =>
                    "Maximum number of barrels that can spawn per day. Increase with luck.\n" +
                    "Decrease when breaking a barrel.\n" +
                    "Default: 2, ignored if the limit is disabled.",
                getValue: () => Config.DailySpawnLimit,
                setValue: value => Config.DailySpawnLimit = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Always Innate Enchantment",
                tooltip: () => "Make sure all weapons drop will have innate enchantment.\nDefault: false",
                getValue: () => Config.AlwaysHaveInnateEnchantment,
                setValue: value => Config.AlwaysHaveInnateEnchantment = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Always Regular Enchantment",
                tooltip: () => "Make sure all weapons drop will have regular enchantment.\nDefault: false",
                getValue: () => Config.AlwaysHaveEnchantment,
                setValue: value => Config.AlwaysHaveEnchantment = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Chance For Enchantment",
                tooltip: () => "The chance a weapon will get a regular enchantment.\nDefault: 0.15",
                getValue: () => Config.ChanceForEnchantment,
                setValue: value => Config.ChanceForEnchantment = value,
                max: 1f,
                min: 0f,
                interval: 0.05f
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Galaxy Check",
                tooltip: () => "Allow galaxy weapons to drop even without getting the galaxy sword before.\nDefault: false",
                getValue: () => Config.SkipGalaxyCheck,
                setValue: value => Config.SkipGalaxyCheck = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Infinity Check",
                tooltip: () => "Allow infinity weapons to drop even without getting an infinity weapon before.\nDefault: false",
                getValue: () => Config.SkipInfinityCheck,
                setValue: value => Config.SkipInfinityCheck = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Allow Global Drop Chance",
                tooltip: () => "If enabled, the drop chance will be the same at any level in the mines/skull cavern.\nInstead of increasing chance at higher levels as it normally would.\nDefault: false",
                getValue: () => Config.HaveGlobalChance,
                setValue: value => Config.HaveGlobalChance = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Base Drop Chance",
                tooltip: () =>
                    "The base chance for a drop.\n" +
                    "Values between 0 and 1 will give a chance between never and always dropping an item.\n" +
                    "Default: 0.6",
                getValue: () => Config.BaseSpawnChance,
                setValue: value => Config.BaseSpawnChance = value,
                max: 1f,
                min: 0f,
                interval: 0.05f
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Drop Chance Increment",
                tooltip: () =>
                    "The value that is added to base drop chance after every 20 floors up to 120.\n" +
                    "At floor 120 the chance is 90%.\n" +
                    "Dangerous mines and Skull Cavern get additional 10%.\n" +
                    "the chance will be increased by 1 (0.10 x 10).\n" +
                    "Ignored if global chance is ON.\n" +
                    "Default: 0.05",
                getValue: () => Config.IncreaseSpawnChanceStep,
                setValue: value => Config.IncreaseSpawnChanceStep = value,
                max: 1f,
                min: 0f,
                interval: 0.05f
            );

            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Chance increase every 20 floors up to 120.\nFrom floor 1 to floor 20 the chance by default is 60% and get to 90% at floor 120 and above."
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Additional Spawn Attempts",
                tooltip: () => "Increase this value to have better chance to spawn a crate per floor.\n\nDefault: 0",
                getValue: () => Config.AdditionalTriesToSpawn,
                setValue: value => Config.AdditionalTriesToSpawn = value,
                min: 0,
                max: 50
            );

            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Doesn't work with the options below."
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Allow More Barrels",
                tooltip: () => "Allow for more than 1 barrel per floor.\nIf value is larger than daily limit will stop spawning after passing it.\nDefault: false",
                getValue: () => Config.AllowMoreThanOne,
                setValue: value => Config.AllowMoreThanOne = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Additional Barrels Amount",
                tooltip: () => "Increase this value to have more barrels per floor.\nDefault: 0",
                getValue: () => Config.AdditionalBarrels,
                setValue: value => Config.AdditionalBarrels = value,
                min: 0,
                max: 20
            );
        }

        /// <summary> Check if player obtained galaxy or infinity weapons</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DailyChecks(object? sender, DayStartedEventArgs e)
        {
            UnlockedGalaxy = Game1.player.mailReceived.Contains("galaxySword");
            UnlockedInfinity = Game1.player.achievements.Contains(42);
            MaxSpawnForDay = Config.DailySpawnLimit + Math.Max(0, Game1.player.LuckLevel);
        }

        /// <summary> Check if player is currently in the mines or skull cavern</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckIfMine(object? sender, WarpedEventArgs e)
        {
            GameLocation location = e.NewLocation;
            if (location is MineShaft mineShaft)
            {
                if (Config.HaveDailySpawnLimit && MaxSpawnForDay <= 0)
                {
                    return;
                }

                if (Config.SkipGalaxyCheck || Config.SkipInfinityCheck)
                {
                    TrySpawnBreakableContainer(mineShaft);
                }
                else if (UnlockedGalaxy || UnlockedInfinity)
                {
                    TrySpawnBreakableContainer(mineShaft);
                }
            }
        }

        /// <summary> Try spawn a "custom" breakable crate </summary>
        /// <param name="mine"></param>
        private void TrySpawnBreakableContainer(MineShaft mine)
        {
            Random mineRandom = new();

            bool shouldSpawnCrate = false;
            int amountToSpawn = 1;

            Layer backLayer = mine.map.GetLayer("Back");
            List<Point> points = new();
            Point p = new(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
            points.Add(p);
            Vector2 objectPos = new(0, 0);
            if (Config.AllowMoreThanOne)
            {
                amountToSpawn += Config.AdditionalBarrels;

                while (amountToSpawn > 0)
                {
                    if (mine.isTileClearForMineObjects(p.X, p.Y))
                    {
                        objectPos = new(p.X, p.Y);
                        mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));
                        amountToSpawn--;
                    }
                    p = new Point(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
                }
                Message = HUDMessage.ForCornerTextbox("Powerful aura is in the air");
                Game1.addHUDMessage(Message);
            }
            else
            {
                int numberOfTries = (int)(mine.mineLevel / 120.0 + Game1.player.DailyLuck + mineRandom.NextDouble());
                for (int i = 0; i < numberOfTries + Config.AdditionalTriesToSpawn; i++)
                {
                    if (mine.isTileClearForMineObjects(p.X, p.Y))
                    {
                        shouldSpawnCrate = true;
                        objectPos = new(p.X, p.Y);
                        break;
                    }
                    else
                    {
                        p = new Point(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
                    }
                }
                if (shouldSpawnCrate)
                {
                    mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));

                    Message = HUDMessage.ForCornerTextbox("Powerful aura is in the air");
                    Game1.addHUDMessage(Message);
                }
            }
        }

    }
}
