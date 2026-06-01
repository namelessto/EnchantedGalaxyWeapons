using EnchantedGalaxyWeapons.Config;
using EnchantedGalaxyWeapons.Core;
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
        private readonly ModConfig _config;

        public int SpawnsRemainingToday { get; private set; }

        public WeaponSpawnService(IModHelper helper, ModConfig config)
        {
            _helper = helper;
            _config = config;
        }

        public void ResetDailyLimit(int luckLevel)
        {
            SpawnsRemainingToday = _config.DailySpawnLimit + Math.Max(0, luckLevel);
        }

        public void DecrementDailyLimit()
        {
            if (!_config.HaveDailySpawnLimit) return;
            SpawnsRemainingToday = Math.Max(0, SpawnsRemainingToday - 1);
            if (SpawnsRemainingToday == 0)
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox(_helper.Translation.Get("game.aura-left")));
        }

        public void TrySpawnBarrel(MineShaft mine)
        {
            bool forced = ModEntry.ForceNextSpawn;
            if (forced) ModEntry.ForceNextSpawn = false;

            if (!forced)
            {
                if (_config.HaveDailySpawnLimit && SpawnsRemainingToday <= 0)
                    return;
                if (!IsUnlockConditionMet())
                    return;
                if (!IsSpawnLocationAllowed(mine))
                    return;
            }

            Random r = Game1.random;

            Layer backLayer = mine.map.GetLayer("Back");
            Point p = new(r.Next(backLayer.LayerWidth), r.Next(backLayer.LayerHeight));
            Vector2 objectPos = Vector2.Zero;
            int tries = Math.Max(1, (int)Math.Ceiling(mine.mineLevel / 120.0 + Game1.player.DailyLuck)) + _config.AdditionalTriesToSpawn;

            if (_config.AdditionalBarrels > 0)
            {
                int amountToSpawn = 1 + _config.AdditionalBarrels;
                int maxAttempts = amountToSpawn * tries;
                int attempts = 0;
                while (amountToSpawn > 0 && attempts++ < maxAttempts)
                {
                    if (mine.isTileClearForMineObjects(p.X, p.Y))
                    {
                        objectPos = new Vector2(p.X, p.Y);
                        mine.objects.Add(objectPos, CustomBreakableObject.GetBarrelForMines(objectPos, mine));
                        amountToSpawn--;
                    }
                    p = new Point(r.Next(backLayer.LayerWidth), r.Next(backLayer.LayerHeight));
                }
                if (objectPos != Vector2.Zero)
                    Game1.addHUDMessage(HUDMessage.ForCornerTextbox(_helper.Translation.Get("game.aura-felt")));
            }
            else
            {
                bool spawned = false;

                for (int i = 0; i < tries; i++)
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
            bool galaxyMet = _config.SkipGalaxyCheck || ModEntry.UnlockedGalaxy;
            bool infinityMet = _config.SkipInfinityCheck || ModEntry.UnlockedInfinity;
            bool modMet = _config.EnabledModWeaponIds.Count > 0 &&
                          Game1.weaponData != null &&
                          Game1.weaponData.Keys.Any(k =>
                              _config.EnabledModWeaponIds.Any(id => WeaponKeyHelper.HasModPrefix(k, id)));
            bool vanillaMet = _config.EnableAllVanillaWeapons;
            return galaxyMet || infinityMet || modMet || vanillaMet;
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
                40 => _config.SpawnInMidMines,
                80 => _config.SpawnInUpperMines,
                _ => false
            };
        }
    }
}
