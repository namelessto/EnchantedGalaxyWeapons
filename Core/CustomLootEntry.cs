namespace EnchantedGalaxyWeapons.Core
{
    public class CustomLootEntry
    {
        public string ItemId { get; set; } = "";
        public float Weight { get; set; } = 1f;
        public int MinStack { get; set; } = 1;
        public int MaxStack { get; set; } = 1;
    }
}
