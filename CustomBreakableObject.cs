using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace EnchantedGalaxyWeapons
{
    internal class CustomBreakableObject : Object
    {
        /*********
        ** Properties
        *********/

        List<string> weaponIDS = new() { "4", "23", "29" };

        Dictionary<int, double> succeedChancesByMineLevel = new Dictionary<int, double>
        {
            { 20, 0.6 },
            { 40, 0.615 },
            { 60, 0.63 },
            { 80, 0.645 },
            { 100, 0.66 },
            { 120, 0.675 },
            { 200, 0.69 },
            { 300, 0.705 },
            { 400, 0.72 }
        };

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

        /// <summary>
        /// Constructors
        /// </summary>
        public CustomBreakableObject() { }
        public CustomBreakableObject(Vector2 tile, string itemId, int health = 3, int debrisType = 12, string hitSound = "woodWhack", string breakSound = "barrelBreak")
    : base(tile, itemId)
        {
            this.health = health;
            this.debris = debrisType;
            this.hitSound = hitSound;
            this.breakSound = breakSound;
            this.breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
            this.breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
        }

        /// <summary>
        /// Pretty much the same as vanilla code but return CustomBreakableObject
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="mine"></param>
        /// <returns></returns>
        public static CustomBreakableObject GetBarrelForMines(Vector2 tile, MineShaft mine)
        {
            int mineArea = mine.getMineArea();
            string itemId = ((mine.GetAdditionalDifficulty() > 0) ? (((mineArea == 0 || mineArea == 10) && !mine.isDarkArea()) ? "262" : "118") : (mineArea switch
            {
                40 => "120",
                80 => "122",
                121 => "124",
                _ => "118",
            }));
            CustomBreakableObject barrel = new(tile, itemId);
            if (Game1.random.NextBool())
            {
                barrel.showNextIndex.Value = true;
            }
            return barrel;
        }

        /// <summary>
        /// Same as vanilla code but without shakeTimer and calls to ReleaseContents in this class
        /// </summary>
        /// <param name="t"></param>
        public override bool performToolAction(Tool t)
        {
            GameLocation location = this.Location;
            if (location == null)
            {
                return false;
            }
            if (t != null && t.isHeavyHitter())
            {
                this.health--;
                if (t is MeleeWeapon weapon && weapon.type.Value == 2)
                {
                    this.health--;
                }
                if (this.health <= 0)
                {
                    if (this.breakSound != null)
                    {
                        base.playNearbySoundAll(this.breakSound);
                    }
                    this.ReleaseContents(t.getLastFarmerToUse());

                    location.objects.Remove(base.TileLocation);
                    int numDebris = Game1.random.Next(4, 12);
                    Color chipColor = this.GetChipColor();
                    for (int i = 0; i < numDebris; i++)
                    {
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value, 999f, 1, 0, base.TileLocation * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (base.TileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
                        {
                            motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
                            acceleration = new Vector2(0f, 0.3f)
                        });
                    }
                }
                else if (this.hitSound != null)
                {
                    base.playNearbySoundAll(this.hitSound);
                    Color? debrisColor = ((base.ItemId == "120") ? new Color?(Color.White) : null);
                    Game1.createRadialDebris(location, this.debris, (int)base.TileLocation.X, (int)base.TileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, debrisColor);
                }
            }
            return false;
        }

        /// <summary>
        /// Same as vanilla code but calls to ReleaseContents in this class
        /// </summary>
        /// <param name="who"></param>
        /// <returns></returns>
        public override bool onExplosion(Farmer who)
        {
            who ??= Game1.player;
            GameLocation location = this.Location;
            if (location == null)
            {
                return true;
            }
            this.ReleaseContents(who);
            int numDebris = Game1.random.Next(4, 12);
            Color chipColor = this.GetChipColor();
            for (int i = 0; i < numDebris; i++)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value, 999f, 1, 0, base.TileLocation * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (base.TileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
                {
                    motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
                    acceleration = new Vector2(0f, 0.3f)
                });
            }
            return true;
        }

        /// <summary>
        /// Change the drop of the barrels to be for the galaxy and infinity weapons.
        /// Allows also to drop Qi beans, Qi gems and special items like books.
        /// Better chance to have a drop with higher mineLevel
        /// </summary>
        /// <param name="who"></param>
        public void ReleaseContents(Farmer who)
        {
            GameLocation location = this.Location;
            if (location == null)
            {
                return;
            }

            if (who == null)
            {
                return;
            }
            Random r = Utility.CreateRandom(base.TileLocation.X, (double)base.TileLocation.Y * 10000.0, Game1.stats.DaysPlayed, (location as MineShaft)?.mineLevel ?? 0);

            int x = (int)base.TileLocation.X;
            int y = (int)base.TileLocation.Y;
            int mineLevel = -1;
            int difficultyLevel = 0;

            if (location is MineShaft mine)
            {
                mineLevel = mine.mineLevel;
                if (mine.isContainerPlatform(x, y))
                {
                    mine.updateMineLevelData(0, -1);
                }
                difficultyLevel = mine.GetAdditionalDifficulty();

                if (mine.mineLevel > 120 && !mine.isSideBranch())
                {
                    int floor = mine.mineLevel - 121;
                    if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
                    {
                        float chance = (float)(floor + Game1.player.team.calicoEggSkullCavernRating.Value * 2) * 0.003f;
                        if (chance > 0.33f)
                        {
                            chance = 0.33f;
                        }
                        if (r.NextBool(chance))
                        {
                            Game1.createMultipleObjectDebris("CalicoEgg", x, y, r.Next(1, 4), who.UniqueMultiplayerID, location);
                        }
                    }
                }
            }

            if (r.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                Game1.createMultipleObjectDebris("(O)890", x, y, r.Next(1, 3), who.UniqueMultiplayerID, location);
            }
            if (Utility.tryRollMysteryBox(0.0081 + Game1.player.team.AverageDailyLuck() / 15.0, r))
            {
                Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(x, y) * 64f + new Vector2(32f), -1, location);
            }
            double maxSucceedChance = 0;

            Utility.trySpawnRareObject(who, new Vector2(x, y) * 64f, location, 1.5, 1.0, -1, r);

            if (difficultyLevel > 0)
            {
                maxSucceedChance += 0.1;
                if (!(r.NextDouble() < 0.15))
                {
                    if (r.NextDouble() < 0.008)
                    {
                        Game1.createMultipleObjectDebris("(O)858", x, y, 1, location);
                    }
                }
            }

            switch (base.ItemId)
            {
                case "118":
                case "120":
                case "122":
                case "124":
                    foreach (var entry in succeedChancesByMineLevel)
                    {
                        if (mineLevel < entry.Key)
                        {
                            maxSucceedChance += entry.Value;
                            break;
                        }
                    }
                    break;
                default:
                    return;
            }
            double dropChance = r.NextDouble();
            if (dropChance <= maxSucceedChance)
            {
                if (ModEntry.unlockedInfinity)
                {
                    weaponIDS.AddRange(new string[] { "62", "63", "64" });
                }

                Item weapon = GenerateWeapon(r);
                Game1.createItemDebris(weapon, new Vector2(x, y) * 64f + new Vector2(32f), r.Next(4), Game1.currentLocation);
                return;
            }

            return;
        }

        /// <summary>
        /// Create a weapon for weaponsIDS list and try to give Innate Enchantment
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Item GenerateWeapon(Random r)
        {
            int itemID = r.Next(weaponIDS.Count);
            Item weapon = new MeleeWeapon(weaponIDS[itemID]);
            weapon = MeleeWeapon.attemptAddRandomInnateEnchantment(weapon, r);
            return weapon;
        }

        /// <summary>
        /// Same as vanilla
        /// </summary>
        /// <returns></returns>
        public Color GetChipColor()
        {
            return base.ItemId switch
            {
                "120" => Color.White,
                "122" => new Color(109, 122, 80),
                "174" => new Color(107, 76, 83),
                _ => new Color(130, 80, 30),
            };
        }
    }
}
