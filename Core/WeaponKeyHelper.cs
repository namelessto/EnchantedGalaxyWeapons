namespace EnchantedGalaxyWeapons.Core
{
    internal static class WeaponKeyHelper
    {
        internal static bool HasModPrefix(string weaponKey, string modId) =>
            weaponKey.StartsWith(modId, StringComparison.OrdinalIgnoreCase) &&
            (weaponKey.Length == modId.Length || !char.IsLetterOrDigit(weaponKey[modId.Length]));
    }
}
