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

    internal static GameObject ChickenPrefab;

    internal static AudioClip ChickenSounds;

    internal static GameObject StarExplosionPrefab;

    internal static AudioClip PolymorphCastSound;

    internal static AudioClip PolymorphSubsideSound;

    internal const float MaxFindTargetAngle = 45f;

    internal static readonly HashSet<int> PolymorphedPlayers = [];

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

        ChickenPrefab = polymorphAssets.LoadAsset<GameObject>("assets/polymorphspell/chicken.prefab");
        Object.DontDestroyOnLoad(ChickenPrefab);

        ChickenSounds = Utils.LoadSound("Chicken.wav", AudioType.WAV);

        StarExplosionPrefab =
            polymorphAssets.LoadAsset<GameObject>(
                "assets/hovl studio/magic effects pack/prefabs/hits and explosions/star hit.prefab");
        Object.DontDestroyOnLoad(StarExplosionPrefab);

        PolymorphCastSound = Utils.LoadSound("Polymorph_Cast.wav", AudioType.WAV);

        PolymorphSubsideSound = Utils.LoadSound("Polymorph_Subside.wav", AudioType.WAV);
    }
}
