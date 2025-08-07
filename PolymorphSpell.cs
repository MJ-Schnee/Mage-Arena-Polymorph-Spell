using BepInEx;
using BepInEx.Logging;
using BlackMagicAPI.Managers;
using UnityEngine;

namespace PolymorphSpell;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("MageArena.exe")]
[BepInDependency("com.magearena.modsync", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.black.magic.api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.mage.configuration.api", BepInDependency.DependencyFlags.SoftDependency)]
public class PolymorphSpell : BaseUnityPlugin
{
    public static PolymorphSpell Instance { get; private set; }

    public static readonly string modsync = "all";

    public static readonly string SpellName = "Polymorph";

    internal static new ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        Instance = this;

        Logger = base.Logger;

        Logger.LogInfo($"Initializing {PluginInfo.PLUGIN_GUID}...");

        PolymorphSpellConfig.LoadConfig(this);

        BlackMagicManager.RegisterSpell(this, typeof(PolymorphSpellData), typeof(PolymorphSpellLogic));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    void Update()
    {
        #if DEBUG
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Utils.SpawnPage(SpellName);
            }
        #endif
    }
}
