﻿using Microsoft.Xna.Framework;
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

        public static bool unlockedGalaxy;
        public static bool unlockedInfinity;

        private int maxSpawnForDay = 0;

        /*********
        ** Public methods
        *********/

        /// <summary> The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>

        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log($"Mod started", LogLevel.Debug);

            helper.Events.GameLoop.DayStarted += this.DailyChecks;
            helper.Events.Player.Warped += this.CheckIfMine;
        }

        /*********
        ** Private methods
        *********/

        /// <summary> Check if player obtained galaxy or infinity weapons</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DailyChecks(object? sender, DayStartedEventArgs e)
        {
            unlockedGalaxy = Game1.player.mailReceived.Contains("galaxySword");
            this.Monitor.Log($"unlockedGalaxy = {Game1.player.mailReceived.Contains("galaxySword")}", LogLevel.Debug);
            unlockedInfinity = Game1.player.achievements.Contains(42);
            this.Monitor.Log($"unlockedInfinity = {Game1.player.achievements.Contains(42)}", LogLevel.Debug);
            maxSpawnForDay = 2;
        }

        /// <summary> Check if player is currently in the mines or skull cavern</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckIfMine(object? sender, WarpedEventArgs e)
        {
            GameLocation location = e.NewLocation;
            if (location is MineShaft mineShaft)
            {
                if (unlockedGalaxy || unlockedInfinity && maxSpawnForDay > 0)
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

            Layer backLayer = mine.map.GetLayer("Back");
            Point p = new(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));

            int numberOfTries = (int)(mine.mineLevel / 120.0 + Game1.player.DailyLuck + mineRandom.NextDouble());

            for (int i = 0; i < numberOfTries; i++)
            {
                if (mine.isTileClearForMineObjects(p.X, p.Y))
                {
                    shouldSpawnCrate = true;
                    break;
                }
                else
                {
                    p = new Point(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
                }
            }

            if (shouldSpawnCrate)
            {
                Vector2 objectPos = new(p.X, p.Y);
                mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));
                maxSpawnForDay--;
                this.Monitor.Log($"Spawn crate", LogLevel.Debug);
            }
        }
    }
}
