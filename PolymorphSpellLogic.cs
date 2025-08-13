using BlackMagicAPI.Modules.Spells;
using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace PolymorphSpell;

internal class PolymorphSpellLogic : SpellLogic
{
    public override void CastSpell(GameObject caster,
        PageController page,
        Vector3 spawnPos,
        Vector3 viewDirectionVector,
        int castingLevel)
    {
        var spellDurationSec = PolymorphSpellConfig.DefaultSpellDurationSecConfig.Value +
                                 castingLevel * PolymorphSpellConfig.CastingLevelDurationIncreaseSecConfig.Value;

        var victim =
            #if DEBUG && SHOULD_POLYMORPH_SELF
                caster;
            #else
                FindTarget(caster);
            #endif

        victim.AddComponent<PolymorphController>();
        
        StartCoroutine(EndPolymorph(victim, spellDurationSec));
    }

    /// <summary>
    /// Finds best player to target
    /// </summary>
    /// <param name="caster"></param>
    /// <returns></returns>
    private static GameObject FindTarget(GameObject caster)
    {
        var casterNetObj = caster.GetComponent<NetworkObject>();
        if (casterNetObj is null)
        {
            PolymorphSpell.Logger.LogError("Spell network object couldn't be found!");
            return null;
        }
        
        var casterNetId = casterNetObj.ObjectId;
        GameObject targetPlayer = null;
        var targetPlayerNetId = 0;
        
        var casterMovementComp = caster.GetComponent<PlayerMovement>();
        if (casterMovementComp is null)
        {
            PolymorphSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
            return null;
        }
        
        var casterPos = caster.transform.position;
        var forward = caster.transform.forward;

        var bestScore = float.MaxValue;

        foreach (var target in GameObject.FindGameObjectsWithTag("Player"))
        {
            // Skip self
            var targetNetObj = target.GetComponent<NetworkObject>();
            if (targetNetObj is null || targetNetObj.ObjectId == casterNetId)
                continue;
            
            // Skip already polymorphed players
            if (PolymorphSpellData.PolymorphedPlayerNetIds.Contains(targetNetObj.ObjectId))
                continue;

            var tempTargetMovement = target.GetComponent<PlayerMovement>();
            if (tempTargetMovement is null)
                continue;

            var toTarget = target.transform.position - casterPos;
            var dist = toTarget.magnitude;
            if (dist > PolymorphSpellConfig.RangeConfig.Value)
                continue;

            var angle = Vector3.Angle(forward, toTarget.normalized);
            if (angle > PolymorphSpellData.MaxFindTargetAngle)
                continue;

            if (!Utils.HasLineOfSight(casterPos, target.transform.position))
                continue;

            var score = angle * 2f + dist;
            if (!(score < bestScore))
                continue;
                
            bestScore = score;
            targetPlayer = tempTargetMovement.gameObject;
            targetPlayerNetId = targetNetObj.ObjectId;
        }
        
        PolymorphSpellData.PolymorphedPlayerNetIds.Add(targetPlayerNetId);
        return targetPlayer;
    }

    /// <summary>
    /// Waits for player to die or polymorph timer to finish and then removes polymorph effect
    /// </summary>
    /// <param name="victim">Victim affected by polymorph</param>
    /// <param name="spellDurationSec">Duration of polymorph</param>
    private static IEnumerator EndPolymorph(GameObject victim, float spellDurationSec)
    {
        var victimPlayerMovement = victim.GetComponent<PlayerMovement>();
        var subsideSoundLength = PolymorphSpellData.PolymorphSubsideSound.length;
        var spellStartTime = Time.time;
        var spellEndTime = spellStartTime + spellDurationSec - subsideSoundLength;

        // Wait until player is dead or polymorph timer has finished
        while (!victimPlayerMovement.isDead)
        {
            if (Time.time < spellEndTime)
            {
                yield return null;
                continue;
            }

            Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphSubsideSound);

            yield return new WaitForSeconds(subsideSoundLength);

            // Player could have died while sound effect playing
            if (!victimPlayerMovement.isDead)
            {
                // Spawn star explosion effects
                var starExplosionEffect = Instantiate(PolymorphSpellData.StarExplosionPrefab, victim.transform.position,
                    Quaternion.identity);
                var effectDuration = starExplosionEffect.GetComponent<ParticleSystem>().main.duration;
                Destroy(starExplosionEffect, effectDuration);
            }

            break;
        }

        // Destroy polymorph
        var polymorphComponent = victim.GetComponent<PolymorphController>();
        if (polymorphComponent is null)
            yield break;
        Destroy(polymorphComponent);
    }
}
