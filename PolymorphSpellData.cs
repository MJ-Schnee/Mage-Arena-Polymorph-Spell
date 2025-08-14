using System.Collections.Generic;
using System.IO;
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

    internal static readonly List<GameObject> PolymorphPrefabs = [];

    internal static GameObject StarExplosionPrefab;

    internal static AudioClip PolymorphCastSound;

    internal static AudioClip PolymorphSubsideSound;

    internal const float MaxFindTargetAngle = 45f;

    internal static readonly HashSet<int> PolymorphedPlayerNetIds = [];

    internal static void LoadAssets()
    {
        var polymorphAssetsPath = Path.Combine(Utils.PluginDir, "AssetBundles", "polymorph");
        var polymorphAssets = BlackMagicAPI.Helpers.Utils.LoadAssetBundleFromDisk(polymorphAssetsPath);
        if (polymorphAssets is null)
        {
            PolymorphSpell.Logger.LogError("Polymorph assets not found");
            return;
        }

        #if DEBUG
            foreach (var asset in polymorphAssets.GetAllAssetNames())
            {
                PolymorphSpell.Logger.LogInfo($"ASSET NAME: {asset}");
            }
        #endif

        var chickenPrefab = polymorphAssets.LoadAsset<GameObject>("assets/polymorphspell/chicken.prefab");
        Object.DontDestroyOnLoad(chickenPrefab);
        PolymorphPrefabs.Add(chickenPrefab);

        var penguinPrefab = polymorphAssets.LoadAsset<GameObject>("assets/polymorphspell/penguin.prefab");
        Object.DontDestroyOnLoad(penguinPrefab);
        PolymorphPrefabs.Add(penguinPrefab);

        var sheepPrefab = polymorphAssets.LoadAsset<GameObject>("assets/polymorphspell/sheep.prefab");
        Object.DontDestroyOnLoad(sheepPrefab);
        PolymorphPrefabs.Add(sheepPrefab);

        var cowPrefab = polymorphAssets.LoadAsset<GameObject>("assets/polymorphspell/cow.prefab");
        Object.DontDestroyOnLoad(cowPrefab);
        PolymorphPrefabs.Add(cowPrefab);

        StarExplosionPrefab =
            polymorphAssets.LoadAsset<GameObject>(
                "assets/hovl studio/magic effects pack/prefabs/hits and explosions/star hit.prefab");
        Object.DontDestroyOnLoad(StarExplosionPrefab);

        PolymorphCastSound = Utils.LoadSound("Polymorph_Cast.wav", AudioType.WAV);

        PolymorphSubsideSound = Utils.LoadSound("Polymorph_Subside.wav", AudioType.WAV);
    }
}
