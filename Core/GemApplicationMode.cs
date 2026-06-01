namespace EnchantedGalaxyWeapons.Core
{
    public enum GemApplicationMode
    {
        Stack,     // one gem type applied N times  →  Ruby×3
        Different, // N different gem types, each once  →  Ruby + Aquamarine + Jade
        Random     // N random picks with replacement  →  Ruby×2 + Aquamarine
    }
}
