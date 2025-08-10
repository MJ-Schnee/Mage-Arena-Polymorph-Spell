using BlackMagicAPI.Modules.Spells;
using System.Collections;
using System.Linq;
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

        var (victim, victimPlayerMovement, victimIsClient) = 
            #if DEBUG && SHOULD_POLYMORPH_SELF
                (caster, caster.GetComponent<PlayerMovement>(), true);
            #else
                FindTarget(caster);
            #endif

        if (victimIsClient)
        {
            PolymorphController.ClientPlayerMovement = victimPlayerMovement;
            victimPlayerMovement.gameObject.AddComponent<PolymorphController>();
        }
        
        StartPolymorph(victim, spellDurationSec);
    }

    /// <summary>
    /// Finds best player to target
    /// </summary>
    /// <param name="caster"></param>
    /// <returns></returns>
    private static (GameObject victim, PlayerMovement victimPlayerMovement, bool victimIsClient)
        FindTarget(GameObject caster)
    {
        var casterNetObj = caster.GetComponent<NetworkObject>();
        if (casterNetObj is null)
        {
            PolymorphSpell.Logger.LogError("Spell network object couldn't be found!");
            return (null, null, false);
        }
        
        var casterNetId = casterNetObj.ObjectId;
        GameObject targetPlayer = null;
        PlayerMovement targetPlayerMovement = null;
        var targetPlayerIsClient = false;
        
        var casterMovementComp = caster.GetComponent<PlayerMovement>();
        if (casterMovementComp is null)
        {
            PolymorphSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
            return (null, null, false);
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
            targetPlayerMovement = tempTargetMovement;
            targetPlayerIsClient = targetNetObj.IsOwner;
        }

        return (targetPlayer, targetPlayerMovement, targetPlayerIsClient);
    }

    /// <summary>
    /// Begin polymorph effect on victim
    /// </summary>
    /// <param name="victim"></param>
    /// <param name="spellDurationSec"></param>
    private void StartPolymorph(GameObject victim, float spellDurationSec)
    {
        var victimPlayerMovement = victim.GetComponent<PlayerMovement>();
        
        Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphCastSound);
        
        // Disable player skins
        var playerSkins = victimPlayerMovement.wizardBody.Where(renderer => renderer.enabled).ToArray();
        foreach (var meshRenderer in playerSkins)
        {
            meshRenderer.enabled = false;
        }
        var arms = victim.transform.Find("armz");
        arms?.gameObject.SetActive(false);
        var pickup = victim.transform.Find("pikupact");
        pickup?.gameObject.SetActive(false);
        
        // Spawn chicken and attach to victim
        var polymorph = Instantiate(PolymorphSpellData.ChickenPrefab, victim.transform.position, Quaternion.identity);
        polymorph.transform.SetParent(victim.transform, false);
        polymorph.transform.localPosition = Vector3.zero;
        polymorph.transform.localRotation = Quaternion.identity;
        
        // Spawn star explosion effects
        var starExplosionEffect = Instantiate(PolymorphSpellData.StarExplosionPrefab, victim.transform.position,
            Quaternion.identity);
        var effectDuration = starExplosionEffect.GetComponent<ParticleSystem>().main.duration;
        Destroy(starExplosionEffect, effectDuration);
        
        // Update client polymorph controller if need be
        if (victim.TryGetComponent<PolymorphController>(out _))
        {
            var polymorphAnimator = polymorph.GetComponent<Animator>();
            if (polymorphAnimator is null)
            {
                PolymorphSpell.Logger.LogError("Polymorph animator not found!");
                return;
            }
            PolymorphController.ClientPolymorphAnimator = polymorphAnimator;
        }

        var spellStartTime = Time.time;
        StartCoroutine(EndPolymorph(victim, spellStartTime, spellDurationSec, polymorph, playerSkins));
    }

    /// <summary>
    /// Waits for player to die or polymorph timer to finish and then removes polymorph effect
    /// </summary>
    /// <param name="victim">Victim affected by polymorph</param>
    /// <param name="spellStartTime">Time when polymorph spell started</param>
    /// <param name="spellDurationSec">Duration of polymorph</param>
    /// <param name="polymorph">Polymorph object</param>
    /// <param name="playerSkins">Disabled skins of victim</param>
    private IEnumerator EndPolymorph(GameObject victim,
        float spellStartTime,
        float spellDurationSec,
        GameObject polymorph,
        SkinnedMeshRenderer[] playerSkins)
    {
        var victimPlayerMovement = victim.GetComponent<PlayerMovement>();
        
        var soundLength = PolymorphSpellData.PolymorphSubsideSound.length;
        
        // Wait until player is dead or polymorph timer has finished
        while (!victimPlayerMovement.isDead)
        {
            if (Time.time < spellStartTime + spellDurationSec - soundLength)
            {
                yield return null;
            }
            
            Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphSubsideSound);

            yield return new WaitForSeconds(soundLength);
            break;
        }
        
        // Spawn star explosion effects
        var starExplosionEffect = Instantiate(PolymorphSpellData.StarExplosionPrefab, victim.transform.position,
            Quaternion.identity);
        var effectDuration = starExplosionEffect.GetComponent<ParticleSystem>().main.duration;
        Destroy(starExplosionEffect, effectDuration);
        
        // Destroy polymorph
        Destroy(polymorph);

        // Re-enable player's skins (player could have died while waiting for sound to finish)
        if (!victimPlayerMovement.isDead)
        {
            var arms = victim.transform.Find("armz");
            var pickup = victim.transform.Find("pikupact");
            arms?.gameObject.SetActive(true);
            pickup?.gameObject.SetActive(true);
            foreach (var meshRenderer in playerSkins)
            {
                meshRenderer.enabled = true;
            }   
        }
        
        // Return camera to default if client
        var clientWasPolymorphed =
            victimPlayerMovement.gameObject.TryGetComponent<PolymorphController>(out var polymorphCamController); 
        if (!clientWasPolymorphed)
        {
            PolymorphSpell.Logger.LogInfo("Polymorphed Camera Controller not found. Assuming client isn't polymorphed");
            yield break;
        }
        Destroy(polymorphCamController);
    }
}
