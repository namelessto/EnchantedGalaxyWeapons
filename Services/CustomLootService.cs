using EnchantedGalaxyWeapons.Config;
using StardewValley;

namespace EnchantedGalaxyWeapons.Services
{
    internal sealed class CustomLootService
    {
        private readonly ModConfig _config;
        private List<string>? _poolCache;

        public CustomLootService(ModConfig config)
        {
            _config = config;
        }

        public void InvalidatePool() => _poolCache = null;

        public Item? RollLoot(Random r)
        {
            if (!_config.EnableCustomLoot) return null;
            if (r.NextDouble() > _config.CustomLootChance) return null;

            _poolCache ??= BuildPool();
            if (_poolCache.Count == 0) return null;

            string id = _poolCache[r.Next(_poolCache.Count)];
            int quantity = _config.LootMinStack >= _config.LootMaxStack
                ? _config.LootMinStack
                : r.Next(_config.LootMinStack, _config.LootMaxStack + 1);

            return ItemRegistry.Create(id, quantity);
        }

        private List<string> BuildPool()
        {
            var pool = new HashSet<string>(_config.LootItems, StringComparer.OrdinalIgnoreCase);

            bool useValueFilter = _config.LootMinValue > 0 || _config.LootMaxValue > 0;
            if (useValueFilter && Game1.objectData != null)
            {
                foreach (var (key, data) in Game1.objectData)
                {
                    if (_config.LootMinValue > 0 && data.Price < _config.LootMinValue) continue;
                    if (_config.LootMaxValue > 0 && data.Price > _config.LootMaxValue) continue;
                    pool.Add($"(O){key}");
                }
            }

            return pool.ToList();
        }
    }
}
