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
        
        // TODO: EndPolymorph on player death
        StartCoroutine(EndPolymorph(victim, spellDurationSec, polymorph, playerSkins));
    }

    private static IEnumerator EndPolymorph(GameObject victim,
        float spellDurationSec,
        GameObject polymorph,
        SkinnedMeshRenderer[] playerSkins)
    {
        var victimPlayerMovement = victim.GetComponent<PlayerMovement>();
        
        var soundLength = PolymorphSpellData.PolymorphSubsideSound.length;
        
        yield return new WaitForSeconds(spellDurationSec - soundLength);
        
        Utils.PlaySpatialSoundAtPosition(victim.transform.position, PolymorphSpellData.PolymorphSubsideSound);

        yield return new WaitForSeconds(soundLength);
        
        // Destroy polymorph
        Destroy(polymorph);

        // Re-enable player's skins
        var arms = victim.transform.Find("armz");
        var pickup = victim.transform.Find("pikupact");
        arms?.gameObject.SetActive(true);
        pickup?.gameObject.SetActive(true);
        foreach (var meshRenderer in playerSkins)
        {
            meshRenderer.enabled = true;
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
