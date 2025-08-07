using BlackMagicAPI.Modules.Spells;
using System.Collections;
using UnityEngine;

namespace PolymorphSpell;

internal class PolymorphSpellLogic : SpellLogic
{
    public override void CastSpell(GameObject caster, PageController page, Vector3 spawnPos, Vector3 viewDirectionVector, int castingLevel)
    {
        var spellDurationSec = PolymorphSpellConfig.DefaultSpellDurationSecConfig.Value +
                                 (castingLevel * PolymorphSpellConfig.CastingLevelDurationIncreaseSecConfig.Value);

        StartPolymorph(caster);
        StartCoroutine(EndPolymorph(caster, spellDurationSec));
    }

    private static void StartPolymorph(GameObject victim)
    {
        Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphCastSound);
        var chickenGameObject = 
            Instantiate(PolymorphSpellData.ChickenPrefab, victim.transform.position, Quaternion.identity);
    }

    private static IEnumerator EndPolymorph(GameObject victim, float spellDurationSec)
    {
        // Unmorph at correct time according to sound
        var soundLength = PolymorphSpellData.PolymorphSubsideSound.length;
        var waitTime = spellDurationSec - soundLength / 2; 
        yield return new WaitForSeconds(waitTime);
        Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphSubsideSound);
        yield return new WaitForSeconds(soundLength / 2);
    }
}
