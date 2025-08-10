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

        // TODO: Cast on other player
        var victim = caster;
        var victimPlayerMovement = victim.GetComponent<PlayerMovement>();
        if (victimPlayerMovement is null)
        {
            PolymorphSpell.Logger.LogError("Victim PlayerMovement not found!");
            return;
        }
        
        // Take over camera controls if client is victim
        var victimNetObj = victimPlayerMovement.GetComponent<NetworkObject>();
        if (victimNetObj is null)
        {
            PolymorphSpell.Logger.LogError("Victim Network Object not found!");
            return;
        }

        if (victimNetObj.IsOwner)
        {
            PolymorphController.IsClientPolymorphed = true;
            PolymorphController.ClientPlayerMovement = victimPlayerMovement;
            victimPlayerMovement.gameObject.AddComponent<PolymorphController>();
        }
        
        StartPolymorph(victim, spellDurationSec);
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
        if (PolymorphController.IsClientPolymorphed)
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
        if (!PolymorphController.IsClientPolymorphed)
            yield break;
        var polymorphCamController = victimPlayerMovement.gameObject.GetComponent<PolymorphController>();
        if (polymorphCamController is null)
        {
            PolymorphSpell.Logger.LogError("Polymorphed Camera Controller not found!");
            yield break;
        }
        Destroy(polymorphCamController);
        PolymorphController.ClientPlayerMovement.ResetCam();
        PolymorphController.ClientPolymorphAnimator = null;
        PolymorphController.IsClientPolymorphed = false;
    }
}
