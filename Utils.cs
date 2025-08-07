using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace PolymorphSpell;

internal static class Utils
{
    public static readonly string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    /// <summary>
    /// Determines if there is a clear line of sight between two positions using multiple raycasts
    /// with slight horizontal offsets to simulate a wider "vision cone".
    /// </summary>
    /// <param name="origin">The world position of viewer.</param>
    /// <param name="target">The world position of target.</param>
    /// <returns>True if at least one of the raycasts from origin to target is unobstructed; otherwise, false.</returns>
    public static bool HasLineOfSight(Vector3 origin, Vector3 target)
    {
        Vector3 eyeOrigin = origin + (Vector3.up * 1.5f);
        Vector3 eyeTarget = target + (Vector3.up * 1.5f);
        Vector3 dir = (eyeTarget - eyeOrigin).normalized;
        float distance = Vector3.Distance(eyeOrigin, eyeTarget);

        // Ignore player layer
        int mask = ~(1 << LayerMask.NameToLayer("Player"));

        float[] angleOffsets = [-5f, 0f, 5f];
        foreach (float offset in angleOffsets)
        {
            Vector3 offsetDir = Quaternion.Euler(0f, offset, 0f) * dir;
            if (!Physics.Raycast(eyeOrigin, offsetDir, distance, mask))
            {
                // At least one clear ray
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Loads sound into audio clip
    /// </summary>
    /// <param name="soundFileName">File name of sound to load</param>
    /// <param name="soundFileType">File type of sound to load</param>
    /// <returns>AudioClip containing sound</returns>
    public static AudioClip LoadSound(string soundFileName, AudioType soundFileType)
    {
        PolymorphSpell.Logger.LogInfo($"Loading sound file: {soundFileName}");
        
        string audioPath = Path.Combine(PluginDir, "Sounds", soundFileName);

        if (!File.Exists(audioPath))
        {
            PolymorphSpell.Logger.LogError($"Could not find sound file: {audioPath}");
            return null;
        }

        string uri = "file://" + audioPath.Replace("\\", "/");

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, soundFileType);
        www.SendWebRequest();

        while (!www.isDone) { }

        if (www.result != UnityWebRequest.Result.Success)
        {
            PolymorphSpell.Logger.LogError($"Failed to load {audioPath}: {www.error}");
            return null;
        }

        PolymorphSpell.Logger.LogInfo($"Successfully loaded {audioPath}");

        return DownloadHandlerAudioClip.GetContent(www);
    }

    /// <summary>
    /// Plays 3D spatial audio at location
    /// </summary>
    /// <param name="position">Position to play audio at</param>
    /// <param name="clip">Audio to play</param>
    public static void PlaySpatialSoundAtPosition(Vector3 position, AudioClip clip)
    {
        if (clip == null)
            return;

        GameObject soundObj = new("PolymorphSpellAudio");
        soundObj.transform.position = position;

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 1f;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 5f;
        source.maxDistance = 350f;
        source.Play();

       Object.Destroy(soundObj, clip.length + 0.1f);
    }

    /// <summary>
    /// Spawns an instance of specified spell's page
    /// </summary>
    /// <param name="spellName">Name of spell to spawn</param>
    public static void SpawnPage(string spellName)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            PolymorphSpell.Logger.LogError("This must be run on the server.");
            return;
        }

        PageLootTable lootTable = Object.FindFirstObjectByType<PageLootTable>();
        if (lootTable == null || lootTable.Pages == null || lootTable.Pages.Length == 0)
        {
            PolymorphSpell.Logger.LogError("PageLootTable is missing or empty.");
            return;
        }

        GameObject prefab = lootTable.Pages
                            .Where(page => page.name == $"Page{spellName}")
                            .FirstOrDefault();
        if (prefab == null)
        {
            PolymorphSpell.Logger.LogError("Page could not be found.");
            return;
        }

        if (prefab.GetComponent<NetworkObject>() == null)
        {
            PolymorphSpell.Logger.LogError($"Prefab '{prefab.name}' is missing a NetworkObject component.");
            return;
        }

        List<GameObject> players =
            [..GameObject.FindGameObjectsWithTag("Player").Where(player => player.name.Contains("Player"))];
        if (players.Count == 0)
        {
            PolymorphSpell.Logger.LogError("No players found.");
            return;
        }

        foreach (GameObject player in players)
        {
            Vector3 spawnPos = player.transform.position + player.transform.forward;
            spawnPos.y += 1.5f;
            GameObject instance = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(instance);

            PolymorphSpell.Logger.LogMessage($"[SERVER] Spawned page '{prefab.name}' for player '{player.name}' at {spawnPos}");
        }
    }
}
