namespace EnchantedGalaxyWeapons.Core
{
    // Maps to SDV innate enchantment classes (verified from MeleeWeapon.attemptAddRandomInnateEnchantment)
    public enum InnateType
    {
        Defense,        // DefenseEnchantment
        Weight,         // LightweightEnchantment
        SlimeGatherer,  // SlimeGathererEnchantment
        SlimeSlayer,    // SlimeSlayerEnchantment
        CritPower,      // CritPowerEnchantment
        CritChance,     // CritEnchantment
        Attack,         // AttackEnchantment
        Speed           // WeaponSpeedEnchantment
    }
}
