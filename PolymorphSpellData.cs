using BlackMagicAPI.Modules.Spells;
using UnityEngine;

namespace PolymorphSpell;

internal class PolymorphSpellData : SpellData
{
    public override string Name => PolymorphSpell.SpellName;

    public override float Cooldown => PolymorphSpellConfig.CooldownConfig.Value;

    public override Color GlowColor => new(1f, 0.321f, 0.498f); // Hot Pink-ish

    public override bool CanSpawnInTeamChest => PolymorphSpellConfig.TeamChestConfig.Value;

    #if DEBUG
        public override bool DebugForceSpawn => true;
    #endif
}
