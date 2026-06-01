using EnchantedGalaxyWeapons.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Layers;

namespace EnchantedGalaxyWeapons.Menu
{
    internal sealed class DevMenu : IClickableMenu
    {
        // Layout constants
        private const int MenuW = 980;
        private const int Pad = 30;
        private const int TitleH = 40;
        private const int StatusH = 92;
        private const int BtnH = 56;
        private const int RowGap = 12;
        private const int ColGap = 10;
        private const int Rows = 5;

        private static int InnerW => MenuW - Pad * 2;
        private static int HalfW => (InnerW - ColGap) / 2;
        private static int MenuH => Pad + TitleH + StatusH + Rows * (BtnH + RowGap) - RowGap + Pad;

        // Buttons — 2 columns × 5 rows
        private readonly ClickableComponent _btnGalaxy, _btnInfinity;
        private readonly ClickableComponent _btnWarp40, _btnWarp80;
        private readonly ClickableComponent _btnWarp100, _btnWarpSC;
        private readonly ClickableComponent _btnSpawn, _btnGiveWeapon;
        private readonly ClickableComponent _btnForce, _btnReset;

        public DevMenu()
        {
            int mx = (Game1.uiViewport.Width - MenuW) / 2;
            int my = (Game1.uiViewport.Height - MenuH) / 2;
            initialize(mx, my, MenuW, MenuH, showUpperRightCloseButton: true);

            (_btnGalaxy, _btnInfinity) = Row(0);
            (_btnWarp40, _btnWarp80) = Row(1);
            (_btnWarp100, _btnWarpSC) = Row(2);
            (_btnSpawn, _btnGiveWeapon) = Row(3);
            (_btnForce, _btnReset) = Row(4);
        }

        private (ClickableComponent left, ClickableComponent right) Row(int row)
        {
            int y = yPositionOnScreen + Pad + TitleH + StatusH + row * (BtnH + RowGap);
            int lx = xPositionOnScreen + Pad;
            int rx = lx + HalfW + ColGap;
            return (
                new ClickableComponent(new Rectangle(lx, y, HalfW, BtnH), $"L{row}"),
                new ClickableComponent(new Rectangle(rx, y, HalfW, BtnH), $"R{row}")
            );
        }

        // ── Draw ─────────────────────────────────────────────────────────────

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.45f);
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            DrawTitle(b);
            DrawStatus(b);

            DrawBtn(b, _btnGalaxy, $"Galaxy Sword: {Flag(ModEntry.UnlockedGalaxy)}",
                ModEntry.UnlockedGalaxy ? Color.SeaGreen : Color.Firebrick);
            DrawBtn(b, _btnInfinity, $"Infinity Weapons: {Flag(ModEntry.UnlockedInfinity)}",
                ModEntry.UnlockedInfinity ? Color.SeaGreen : Color.Firebrick);

            DrawBtn(b, _btnWarp40, "Warp: Floor 40", new Color(70, 110, 160));
            DrawBtn(b, _btnWarp80, "Warp: Floor 80", new Color(70, 110, 160));
            DrawBtn(b, _btnWarp100, "Warp: Floor 100", new Color(70, 110, 160));
            DrawBtn(b, _btnWarpSC, "Warp: Skull Cavern", new Color(140, 60, 160));

            bool inMine = Game1.currentLocation is MineShaft;
            DrawBtn(b, _btnSpawn, "Spawn Barrel Here",
                inMine ? new Color(50, 130, 80) : Color.Gray);
            DrawBtn(b, _btnGiveWeapon, "Give Weapon", new Color(50, 130, 80));

            DrawBtn(b, _btnForce, $"Force Next Spawn: {(ModEntry.ForceNextSpawn ? "ON" : "OFF")}",
                ModEntry.ForceNextSpawn ? Color.DarkGoldenrod : new Color(80, 80, 80));
            DrawBtn(b, _btnReset, $"Reset Limit ({ModEntry.SpawnService.SpawnsRemainingToday} left)",
                new Color(80, 80, 80));

            base.draw(b);
            drawMouse(b);
        }

        private void DrawTitle(SpriteBatch b)
        {
            Utility.drawTextWithShadow(b, "EGW Dev Tools",
                Game1.dialogueFont,
                new Vector2(xPositionOnScreen + Pad, yPositionOnScreen + Pad),
                Game1.textColor);
        }

        private void DrawStatus(SpriteBatch b)
        {
            int sx = xPositionOnScreen + Pad;
            int sy = yPositionOnScreen + Pad + TitleH;
            Color col = Game1.textColor * 0.85f;
            SpriteFont font = Game1.smallFont;

            Utility.drawTextWithShadow(b, LocationLine(), font, new Vector2(sx, sy), col);
            Utility.drawTextWithShadow(b, StatsLine(), font, new Vector2(sx, sy + 30), col);
            Utility.drawTextWithShadow(b, BarrelSpawnLine(), font, new Vector2(sx, sy + 60), col);
        }

        private static void DrawBtn(SpriteBatch b, ClickableComponent btn, string label, Color color)
        {
            bool hovered = btn.bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
            b.Draw(Game1.fadeToBlackRect, btn.bounds, hovered ? color : color * 0.72f);

            Vector2 sz = Game1.dialogueFont.MeasureString(label);
            Vector2 pos = new(
                btn.bounds.X + (btn.bounds.Width - sz.X) / 2f,
                btn.bounds.Y + (btn.bounds.Height - sz.Y) / 2f);
            Utility.drawTextWithShadow(b, label, Game1.dialogueFont, pos, Color.White);
        }

        // ── Click ─────────────────────────────────────────────────────────────

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (_btnGalaxy.containsPoint(x, y)) ToggleGalaxy();
            else if (_btnInfinity.containsPoint(x, y)) ToggleInfinity();
            else if (_btnWarp40.containsPoint(x, y)) WarpToFloor(40);
            else if (_btnWarp80.containsPoint(x, y)) WarpToFloor(80);
            else if (_btnWarp100.containsPoint(x, y)) WarpToFloor(100);
            else if (_btnWarpSC.containsPoint(x, y)) WarpToFloor(121);
            else if (_btnSpawn.containsPoint(x, y)) TrySpawnBarrel();
            else if (_btnGiveWeapon.containsPoint(x, y)) GiveWeapon();
            else if (_btnForce.containsPoint(x, y)) ToggleForceSpawn();
            else if (_btnReset.containsPoint(x, y)) ResetLimit();
        }

        // ── Actions ───────────────────────────────────────────────────────────

        private static void ToggleGalaxy()
        {
            if (ModEntry.UnlockedGalaxy) Game1.player.mailReceived.Remove("galaxySword");
            else Game1.player.mailReceived.Add("galaxySword");
            Game1.playSound("coin");
        }

        private static void ToggleInfinity()
        {
            if (ModEntry.UnlockedInfinity) Game1.player.achievements.Remove(42);
            else Game1.player.achievements.Add(42);
            Game1.playSound("coin");
        }

        private static void WarpToFloor(int floor)
        {
            Game1.warpFarmer(Game1.getLocationRequest("UndergroundMine" + floor), 6, 6, 2);
        }

        private static void TrySpawnBarrel()
        {
            if (Game1.currentLocation is not MineShaft mine)
            {
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox("Must be in a mine or skull cavern."));
                return;
            }

            Layer? backLayer = mine.map?.GetLayer("Back");
            if (backLayer is null)
            {
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox("Mine map not ready."));
                return;
            }

            for (int i = 0; i < 200; i++)
            {
                int x = Game1.random.Next(backLayer.LayerWidth);
                int y = Game1.random.Next(backLayer.LayerHeight);
                if (!mine.isTileClearForMineObjects(x, y)) continue;

                Vector2 tile = new(x, y);
                mine.objects.Add(tile, CustomBreakableObject.GetBarrelForMines(tile, mine));
                Game1.playSound("axchop");
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox("Barrel spawned."));
                return;
            }

            Game1.addHUDMessage(HUDMessage.ForCornerTextbox("No clear tile found after 200 attempts."));
        }

        private void GiveWeapon()
        {
            var weapon = ModEntry.GenerationService.GenerateWeapon(
                Game1.random, ModEntry.UnlockedGalaxy, ModEntry.UnlockedInfinity);

            if (weapon == null)
            {
                Game1.addHUDMessage(HUDMessage.ForCornerTextbox("No weapons in pool — check unlock flags or config."));
                return;
            }

            Game1.player.addItemByMenuIfNecessary(weapon);
            Game1.playSound("coin");
            exitThisMenu();
        }

        private static void ToggleForceSpawn()
        {
            ModEntry.ForceNextSpawn = !ModEntry.ForceNextSpawn;
            Game1.playSound("drumkit6");
        }

        private static void ResetLimit()
        {
            ModEntry.SpawnService.ResetDailyLimit(Game1.player.LuckLevel);
            Game1.playSound("coin");
        }

        // ── Status helpers ────────────────────────────────────────────────────

        private static string LocationLine()
        {
            if (Game1.currentLocation is not MineShaft mine)
                return "Location: Not in a mine";

            string area = mine.getMineArea() switch
            {
                0 or 10 => "Lower Mines",
                40 => "Mid Mines",
                80 => "Upper Mines",
                121 => "Skull Cavern",
                _ => "Unknown Area"
            };

            string danger = mine.GetAdditionalDifficulty() > 0 ? " (Dangerous)" : "";
            return $"Location: {area}{danger}  —  Floor {mine.mineLevel}";
        }

        private static string StatsLine()
        {
            float chance = ModEntry.Config.BaseSpawnChance;
            int mineLevel = (Game1.currentLocation as MineShaft)?.mineLevel ?? -1;

            if (!ModEntry.Config.HaveGlobalChance && mineLevel > 0)
            {
                foreach (int step in new[] { 20, 40, 60, 80, 100, 120 })
                    if (mineLevel >= step)
                        chance += ModEntry.Config.IncreaseSpawnChanceStep;
            }

            string spawnStr = ModEntry.Config.HaveDailySpawnLimit
                ? $"{ModEntry.SpawnService.SpawnsRemainingToday} remaining"
                : "unlimited";

            string luckStr = Game1.player.DailyLuck.ToString("+0.00;-0.00;0.00");
            return $"Drop chance: {Math.Round(chance * 100)}%   |   Spawns today: {spawnStr}   |   Luck: {luckStr}";
        }

        private static string BarrelSpawnLine()
        {
            if (Game1.currentLocation is not MineShaft mine)
                return "Barrel spawn: N/A";

            Layer? backLayer = mine.map?.GetLayer("Back");
            if (backLayer is null)
                return "Barrel spawn: N/A";

            int total = backLayer.LayerWidth * backLayer.LayerHeight;
            int clear = 0;
            for (int x = 0; x < backLayer.LayerWidth; x++)
                for (int y = 0; y < backLayer.LayerHeight; y++)
                    if (mine.isTileClearForMineObjects(x, y))
                        clear++;

            if (total == 0)
                return "Barrel spawn: N/A";

            double p = (double)clear / total;
            int n = Math.Max(1, (int)Math.Ceiling(mine.mineLevel / 120.0 + Game1.player.DailyLuck)) + ModEntry.Config.AdditionalTriesToSpawn;
            double chance = 1.0 - Math.Pow(1.0 - p, n);

            return $"Barrel spawn: ~{Math.Round(chance * 100)}%  ({clear}/{total} tiles clear, {n} tries)";
        }

        private static string Flag(bool on) => on ? "Unlocked" : "Locked";
    }
}
