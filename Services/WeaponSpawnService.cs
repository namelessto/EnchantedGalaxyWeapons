using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Objects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Layers;

namespace EnchantedGalaxyWeapons.Services
{
    internal sealed class WeaponSpawnService
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;

        public int SpawnsRemainingToday { get; private set; }

        public WeaponSpawnService(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;
        }

        public void ResetDailyLimit(int luckLevel)
        {
            SpawnsRemainingToday = _config.DailySpawnLimit + Math.Max(0, luckLevel);
        }

        public void DecrementDailyLimit()
        {
            SpawnsRemainingToday = Math.Max(0, SpawnsRemainingToday - 1);
        }

        public void TrySpawnBarrel(MineShaft mine)
        {
            if (_config.HaveDailySpawnLimit && SpawnsRemainingToday <= 0)
                return;

            if (!IsUnlockConditionMet())
                return;

            if (!IsSpawnLocationAllowed(mine))
                return;

            // Seeded random: deterministic per day + floor + save
            Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, mine.mineLevel, Game1.uniqueIDForThisGame);

            Layer backLayer = mine.map.GetLayer("Back");
            Point p = new(r.Next(backLayer.LayerWidth), r.Next(backLayer.LayerHeight));
            Vector2 objectPos = Vector2.Zero;

            if (_config.AllowMoreThanOne)
            {
                int amountToSpawn = 1 + _config.AdditionalBarrels;
                while (amountToSpawn > 0)
                {
                    if (mine.isTileClearForMineObjects(p.X, p.Y))
                    {
                        objectPos = new Vector2(p.X, p.Y);
                        mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));
                        amountToSpawn--;
                    }
                    p = new Point(r.Next(backLayer.LayerWidth), r.Next(backLayer.LayerHeight));
                }
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox(_helper.Translation.Get("game.aura-felt")));
            }
            else
            {
                int numberOfTries = (int)(mine.mineLevel / 120.0 + Game1.player.DailyLuck + r.NextDouble());
                bool spawned = false;

                for (int i = 0; i < numberOfTries + _config.AdditionalTriesToSpawn; i++)
                {
                    if (mine.isTileClearForMineObjects(p.X, p.Y))
                    {
                        objectPos = new Vector2(p.X, p.Y);
                        spawned = true;
                        break;
                    }
                    p = new Point(r.Next(backLayer.LayerWidth), r.Next(backLayer.LayerHeight));
                }

                if (spawned)
                {
                    mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));
                    Game1.addHUDMessage(HUDMessage.ForCornerTextbox(_helper.Translation.Get("game.aura-felt")));
                }
            }
        }

        private bool IsUnlockConditionMet()
        {
            if (_config.SkipGalaxyCheck || _config.SkipInfinityCheck)
                return true;
            return ModEntry.UnlockedGalaxy || ModEntry.UnlockedInfinity;
        }

        private bool IsSpawnLocationAllowed(MineShaft mine)
        {
            bool isDangerous = mine.GetAdditionalDifficulty() > 0;

            if (mine.mineLevel > 120)
                return _config.SpawnInSkullCavern;

            if (isDangerous)
                return _config.SpawnInDangerousMines;

            return mine.getMineArea() switch
            {
                0 or 10 => _config.SpawnInLowerMines,
                40      => _config.SpawnInMidMines,
                80      => _config.SpawnInUpperMines,
                _       => false
            };
        }
    }
}
