using System.Collections.Generic;

namespace AdvancedTooltip
{
    internal static class ShortModNames
    {
        internal static readonly Dictionary<string, string> shortModNames = new Dictionary<string, string>()
        {
            // Physical Damage
            { "PhysicalDamage", "Flat Phys" },
            { "LocalPhysicalDamagePercent", "%-Phys" },
            { "LocalIncreasedPhysicalDamagePercentAndAccuracyRating", "Hybrid Phys" },
            
            // Attack & Cast Speed
            { "IncreasedAttackSpeed", "Attack Speed" },
            { "IncreasedCastSpeed", "Cast Speed" },
            
            // Elemental Damage
            { "FireDamage", "Flat Fire" },
            { "ColdDamage", "Flat Cold" },
            { "LightningDamage", "Flat Lightning" },
            { "FireDamagePercentage", "%-Fire" },
            { "ColdDamagePercentage", "%-Cold" },
            { "LightningDamagePercentage", "%-Lightning" },
            { "IncreasedWeaponElementalDamagePercent", "Ele Damage" },
            
            // Critical Strike
            { "CriticalStrikeChanceIncrease", "Crit Chance" },
            { "CriticalStrikeMultiplier", "Crit Multi" },
            { "SpellCriticalStrikeChanceIncrease", "Spell Crit" },
            
            // Accuracy
            { "IncreasedAccuracy", "Accuracy" },
            
            // Defenses
            { "BaseLocalDefences", "Flat Defence" },
            { "DefencesPercent", "%-Defence" },
            { "BaseLocalDefencesAndDefencePercent", "Hybrid Defence" },
            { "DefencesPercentAndStunThreshold", "Hybrid Stun" },
            { "BaseLocalDefencesAndLife", "Hybrid Life" },
            { "BaseLocalDefencesAndMana", "Hybrid Mana" },
            
            // Armour
            { "IncreasedPhysicalDamageReductionRating", "Flat Armour" },
            { "IncreasedPhysicalDamageReductionRatingPercent", "%-Armour" },
            
            // Evasion
            { "IncreasedEvasionRating", "Flat Evasion" },
            { "EvasionRatingPercent", "%-Evasion" },
            
            // Energy Shield
            { "EnergyShieldPercent", "%-ES" },
            { "MaximumEnergyShield", "Flat ES" },
            
            // Resistances
            { "FireResistance", "Fire Res" },
            { "ColdResistance", "Cold Res" },
            { "LightningResistance", "Lightning Res" },
            { "ChaosResistance", "Chaos Res" },
            { "AllResistances", "All Res" },
            
            // Life & Mana
            { "IncreasedLife", "Life" },
            { "MaximumLifeIncreasePercent", "%-Life" },
            { "IncreasedMana", "Mana" },
            { "ManaRegeneration", "Mana Regen" },
            
            // Attributes
            { "Strength", "Strength" },
            { "Intelligence", "Intelligence" },
            { "Dexterity", "Dexterity" },
            
            // Misc
            { "MovementVelocity", "Movespeed" },
            { "LifeLeech", "Life Leech" },
            { "ManaLeech", "Mana Leech" },
            { "LifeGainedFromEnemyDeath", "Life on Kill" },
            { "ManaGainedFromEnemyDeath", "Mana on Kill" },
            { "ItemFoundRarityIncrease", "Rarity" },
            { "ItemFoundRarityIncreasePrefix", "Rarity" },
            { "IncreaseSocketedGemLevel", "+Gem Level" },
            { "ProjectileSpeed", "Proj Speed" },
            { "ChanceToPierce", "Pierce" },
            { "DamageWithWeaponTypeSkill", "Weapon Damage" },
        };

        internal static string GetByGroup(string x)
        {
            var name = string.Empty;
            if (shortModNames.TryGetValue(x, out var result))
                name = result;
            return name;
        }
    }
}

