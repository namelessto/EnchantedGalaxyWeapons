using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace EnchantedGalaxyWeapons.Objects
{
    internal class CustomBreakableObject : BreakableContainer
    {
        private static readonly List<int> DropStepLevels = new() { 20, 40, 60, 80, 100, 120 };
        private const string ContentSeedKey = "NamelessTo.EnchantedGalaxyWeapons/contentSeed";
        private float _sparkleTimer;
        private int _health = 3;

        public CustomBreakableObject() { }

        public CustomBreakableObject(Vector2 tile, string itemId) : base(tile, itemId)
        {
            this.modData[ContentSeedKey] = Game1.random.Next().ToString();
        }

        public static new CustomBreakableObject GetBarrelForMines(Vector2 tile, MineShaft mine)
        {
            int mineArea = mine.getMineArea();
            string itemId = (mine.GetAdditionalDifficulty() > 0)
                ? (((mineArea == 0 || mineArea == 10) && !mine.isDarkArea()) ? "262" : "118")
                : mineArea switch { 40 => "120", 80 => "122", 121 => "124", _ => "118" };

            CustomBreakableObject barrel = new(tile, itemId);
            if (Game1.random.NextBool())
                barrel.showNextIndex.Value = true;

            return barrel;
        }

        public override bool performToolAction(Tool t)
        {
            if (this.Location == null) return false;
            if (t == null || !t.isHeavyHitter()) return false;

            _health--;
            if (t is MeleeWeapon weapon && weapon.type.Value == 2)
                _health--;

            if (_health <= 0)
            {
                this.playNearbySoundAll("barrelBreak");
                this.releaseContents(t.getLastFarmerToUse());
                this.Location.objects.Remove(TileLocation);
                SpawnDebris(this.Location);
                return true;
            }

            this.playNearbySoundAll("woodWhack");
            Game1.createRadialDebris(this.Location, 12, (int)TileLocation.X, (int)TileLocation.Y, Game1.random.Next(4, 7), resource: false);
            return false;
        }

        public override bool onExplosion(Farmer who)
        {
            who ??= Game1.player;
            GameLocation? location = this.Location ?? Game1.currentLocation;
            if (location == null) return true;

            this.releaseContents(who);
            location.objects.Remove(TileLocation);
            SpawnDebris(location);
            return true;
        }

        // Shadows BreakableContainer.releaseContents (not virtual) — called explicitly from our overrides above.
        public new void releaseContents(Farmer who)
        {
            if (this.Location == null || who == null) return;

            int seed = modData.TryGetValue(ContentSeedKey, out string? raw) && int.TryParse(raw, out int parsed)
                ? parsed
                : (int)(TileLocation.X + TileLocation.Y * 10000 + Game1.stats.DaysPlayed);
            Random r = Utility.CreateRandom(seed);
            int x = (int)TileLocation.X, y = (int)TileLocation.Y;
            int mineLevel = (this.Location as MineShaft)?.mineLevel ?? -1;

            if (r.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                Game1.createMultipleObjectDebris("(O)890", x, y, r.Next(1, 3), who.UniqueMultiplayerID, this.Location);

            double dropChance = r.NextDouble();
            double maxSucceedChance = ModEntry.Config.BaseSpawnChance;

            if (!ModEntry.Config.HaveGlobalChance)
            {
                foreach (int level in DropStepLevels)
                {
                    if (mineLevel >= level)
                        maxSucceedChance += ModEntry.Config.IncreaseSpawnChanceStep;
                }
            }

            bool weaponDropped = false;
            if (dropChance <= maxSucceedChance)
            {
                MeleeWeapon? dropped = ModEntry.GenerationService.GenerateWeapon(r, ModEntry.UnlockedGalaxy, ModEntry.UnlockedInfinity);
                if (dropped != null)
                {
                    Game1.createItemDebris(dropped, new Vector2(x, y) * 64f + new Vector2(32f), r.Next(4), this.Location);
                    ModEntry.SpawnService?.DecrementDailyLimit();
                    weaponDropped = true;
                }
            }

            if (!weaponDropped)
            {
                Item? loot = ModEntry.LootService.RollLoot(r);
                if (loot != null)
                    Game1.createItemDebris(loot, new Vector2(x, y) * 64f + new Vector2(32f), r.Next(4), this.Location);
            }
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            base.updateWhenCurrentLocation(time);

            if (this.Location == null) return;

            _sparkleTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
            if (_sparkleTimer <= 0f)
            {
                _sparkleTimer = Game1.random.Next(300, 700);

                Vector2 origin = TileLocation * 64f + new Vector2(
                    Game1.random.Next(8, 56),
                    Game1.random.Next(8, 56));

                this.Location.temporarySprites.Add(new TemporaryAnimatedSprite(
                    "TileSheets\\animations",
                    new Rectangle(0, 640, 64, 64),
                    80f, 5, 0,
                    origin,
                    flicker: false,
                    Game1.random.NextBool(),
                    (origin.Y + 64f) / 10000f,
                    0.003f,
                    Utility.GetPrismaticColor(speedMultiplier: 5f),
                    0.3f,
                    -0.002f,
                    0f, 0f)
                {
                    motion = new Vector2(
                        RandFloat(Game1.random, -0.3f, 0.3f),
                        RandFloat(Game1.random, -1.5f, -0.5f))
                });
            }
        }

        private new Color GetChipColor() => ItemId switch
        {
            "120" => Color.White,
            "122" => new Color(109, 122, 80),
            "124" => new Color(107, 76, 83),
            _     => new Color(130, 80, 30)
        };

        private void SpawnDebris(GameLocation location)
        {
            int numDebris = Game1.random.Next(4, 12);
            Color chipColor = GetChipColor();

            for (int i = 0; i < numDebris; i++)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(
                    "LooseSprites\\Cursors",
                    Game1.random.NextBool() ? new Rectangle(598, 1275, 13, 4) : new Rectangle(611, 1275, 10, 4),
                    999f, 1, 0,
                    TileLocation * 64f + new Vector2(32f, 32f),
                    flicker: false, Game1.random.NextBool(),
                    (TileLocation.Y * 64f + 32f) / 10000f, 0.01f,
                    chipColor, 4f, 0f,
                    (float)Game1.random.Next(-5, 6) * MathF.PI / 8f,
                    (float)Game1.random.Next(-5, 6) * MathF.PI / 64f)
                {
                    motion       = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
                    acceleration = new Vector2(0f, 0.3f)
                });
            }
        }

        private static float RandFloat(Random r, float min, float max) =>
            (float)(r.NextDouble() * (max - min) + min);
    }
}
