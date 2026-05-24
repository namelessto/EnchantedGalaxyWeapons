using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace EnchantedGalaxyWeapons.Objects
{
    internal class CustomBreakableObject : Object
    {
        private readonly List<int> StepsLevel = new() { 20, 40, 60, 80, 100, 120 };

        [XmlElement("debris")]
        private readonly int debris;

        [XmlElement("health")]
        private new int health;

        [XmlElement("hitSound")]
        private readonly string hitSound = "woodWhack";

        [XmlElement("breakSound")]
        private readonly string breakSound = "barrelBreak";

        [XmlElement("breakDebrisSource")]
        private readonly NetRectangle breakDebrisSource = new();

        [XmlElement("breakDebrisSource2")]
        private readonly NetRectangle breakDebrisSource2 = new();

        private float _sparkleTimer;

        public CustomBreakableObject() { }

        public CustomBreakableObject(Vector2 tile, string itemId, int health = 3, int debrisType = 12,
            string hitSound = "woodWhack", string breakSound = "barrelBreak")
            : base(tile, itemId)
        {
            this.health = health;
            this.debris = debrisType;
            this.hitSound = hitSound;
            this.breakSound = breakSound;
            this.breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
            this.breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
        }

        public static CustomBreakableObject GetBarrelForMines(Vector2 tile, MineShaft mine)
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

            this.health--;
            if (t is MeleeWeapon weapon && weapon.type.Value == 2)
                this.health--;

            if (this.health <= 0)
            {
                if (!string.IsNullOrEmpty(this.breakSound))
                    base.playNearbySoundAll(this.breakSound);

                this.ReleaseContents(t.getLastFarmerToUse());
                this.Location.objects.Remove(base.TileLocation);
                this.SpawnDebris();
            }
            else if (!string.IsNullOrEmpty(this.hitSound))
            {
                base.playNearbySoundAll(this.hitSound);
                Game1.createRadialDebris(this.Location, this.debris, (int)base.TileLocation.X, (int)base.TileLocation.Y, Game1.random.Next(4, 7), resource: false);
            }

            return false;
        }

        public override bool onExplosion(Farmer who)
        {
            who ??= Game1.player;
            if (this.Location == null) return true;

            this.ReleaseContents(who);
            this.SpawnDebris();
            return true;
        }

        public void ReleaseContents(Farmer who)
        {
            if (this.Location == null || who == null) return;

            Random r = Utility.CreateRandom(base.TileLocation.X, base.TileLocation.Y * 10000, Game1.stats.DaysPlayed);
            int x = (int)base.TileLocation.X, y = (int)base.TileLocation.Y;
            int mineLevel = (this.Location as MineShaft)?.mineLevel ?? -1;

            if (r.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                Game1.createMultipleObjectDebris("(O)890", x, y, r.Next(1, 3), who.UniqueMultiplayerID, this.Location);

            double dropChance = r.NextDouble();
            double maxSucceedChance = ModEntry.Config.BaseSpawnChance;

            // When HaveGlobalChance is on, skip floor-based scaling and use BaseSpawnChance flat
            if (!ModEntry.Config.HaveGlobalChance)
            {
                foreach (var level in StepsLevel)
                {
                    if (mineLevel >= level)
                        maxSucceedChance += ModEntry.Config.IncreaseSpawnChanceStep;
                }
            }

            if (dropChance <= maxSucceedChance)
            {
                MeleeWeapon? dropped = ModEntry.GenerationService.GenerateWeapon(r, ModEntry.UnlockedGalaxy, ModEntry.UnlockedInfinity);
                if (dropped != null)
                {
                    Game1.createItemDebris(dropped, new Vector2(x, y) * 64f + new Vector2(32f), r.Next(4), this.Location);
                    ModEntry.SpawnService?.DecrementDailyLimit();
                }
                else
                {
                    var dangerBarrel = new BreakableContainer(base.TileLocation, "262") { Location = this.Location };
                    dangerBarrel.releaseContents(who);
                }
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

                Vector2 origin = this.TileLocation * 64f + new Vector2(
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

        private Color GetChipColor() => base.ItemId switch
        {
            "120" => Color.White,
            "122" => new Color(109, 122, 80),
            "174" => new Color(107, 76, 83),
            _     => new Color(130, 80, 30)
        };

        private void SpawnDebris()
        {
            int numDebris = Game1.random.Next(4, 12);
            Color chipColor = this.GetChipColor();

            for (int i = 0; i < numDebris; i++)
            {
                float rotSpeed = RandFloat(Game1.random, -MathF.PI / 64, MathF.PI / 64);
                float rot = RandFloat(Game1.random, -MathF.PI / 8, MathF.PI / 8);
                float mx = RandFloat(Game1.random, -3f, 3f);
                float my = RandFloat(Game1.random, -1f, -0.7f);

                Game1.Multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite(
                    "LooseSprites\\Cursors",
                    Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value,
                    999f, 1, 0,
                    base.TileLocation * 64f + new Vector2(32f, 32f),
                    flicker: false, Game1.random.NextBool(),
                    (base.TileLocation.Y * 64f + 32f) / 10000f, 0.01f,
                    chipColor, 4f, 0f, rot, rotSpeed)
                {
                    motion = new Vector2(mx, my),
                    acceleration = new Vector2(0f, 0.3f)
                });
            }
        }

        private static float RandFloat(Random r, float min, float max) =>
            (float)(r.NextDouble() * (max - min) + min);
    }
}
