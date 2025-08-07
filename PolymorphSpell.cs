using BepInEx;
using BepInEx.Logging;
using BlackMagicAPI.Helpers;
using BlackMagicAPI.Managers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PolymorphSpell;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("MageArena.exe")]
[BepInDependency("com.magearena.modsync", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.black.magic.api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.mage.configuration.api", BepInDependency.DependencyFlags.SoftDependency)]
public class PolymorphSpell : BaseUnityPlugin
{
    public static readonly string modsync = "all";

    public const string SpellName = "Polymorph";

    internal new static ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo($"Initializing {PluginInfo.PLUGIN_GUID}...");

        PolymorphSpellConfig.LoadConfig(this);

        PolymorphSpellData.PolymorphAssets = Assembly.GetCallingAssembly()
            .LoadAssetBundleFromResources("PolymorphSpell.AssetBundles.Polymorph");

        PolymorphSpellData.PolymorphCastSound = Utils.LoadSound("Polymorph_Cast.wav", AudioType.WAV);

        PolymorphSpellData.PolymorphSubsideSound = Utils.LoadSound("Polymorph_Subside.wav", AudioType.WAV);

        BlackMagicManager.RegisterSpell(this, typeof(PolymorphSpellData), typeof(PolymorphSpellLogic));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        #if DEBUG
            if (Input.GetKeyDown(KeyCode.F4))
            {
                List<GameObject> players =
                    [..GameObject.FindGameObjectsWithTag("Player").Where(player => player.name.Contains("Player"))];
                if (players.Count == 0)
                {
                    Logger.LogError("No players found.");
                    return;
                }

                foreach (var player in players)
                {
                    var spawnPos = player.transform.position + player.transform.forward;
                    spawnPos.y += 1.5f;
                    BlackMagicManager.SpawnSpell<PolymorphSpellLogic>(spawnPos);

                    Logger.LogMessage($"[SERVER] Spawned page '{SpellName}' for player '{player.name}' at {spawnPos}");
                }
            }
        #endif
    }
}
