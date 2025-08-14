using System.Linq;
using System.Reflection;
using Dissonance;
using FishNet.Object;
using UnityEngine;

namespace PolymorphSpell;

/// <summary>
/// Camera controller attached to player when polymorphed
/// </summary>
internal class PolymorphController: MonoBehaviour
{
    private static readonly int PolymorphAnimatorWalkingId = Animator.StringToHash("Vert");

    private static readonly int PolymorphAnimatorRunningId = Animator.StringToHash("State");

    private GameObject _player;

    private PlayerMovement _playerMovement;
    
    private PlayerInventory _playerInventory;

    private NetworkObject _playerNetObj;

    private SkinnedMeshRenderer[] _playerSkins;

    private GameObject _polymorphGameObject;

    private Animator _polymorphAnimator;

    private FieldInfo _playerHealthField;

    private bool _isClient;

    private Camera _clientPlayerCam;

    private Animator _clientArmsAni;

    private DissonanceComms _clientComms;

    private AudioSource _polymorphSoundLoop;

    private float _prePolymorphHealth;

    private void Awake()
    {
        AwakeForAll();

        if (!_playerNetObj.IsOwner)
            return;

        AwakeForLocal();
    }

    private void Update()
    {
        // Update polymorph animator
        _polymorphAnimator.SetFloat(PolymorphAnimatorWalkingId,
            _playerMovement.currentSpeed / _playerMovement.walkingSpeed);
        _polymorphAnimator.SetFloat(PolymorphAnimatorRunningId,
            _playerMovement.currentSpeed > _playerMovement.walkingSpeed
                ? _playerMovement.currentSpeed / _playerMovement.runningSpeed
                : 0f);

        // Ensure health doesn't go over polymorph max
        if ((float)_playerHealthField.GetValue(_playerMovement) > PolymorphSpellConfig.PolymorphHealth.Value)
            _playerHealthField.SetValue(_playerMovement, PolymorphSpellConfig.PolymorphHealth.Value);

        // The rest of update is only for the client
        if (!_isClient)
            return;
        
        // Disable recall
        _playerMovement.canRecall = false;
        
        // Disable crouch
        var playerIsCrouch = typeof(PlayerMovement).GetField("isCrouch", BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerIsCrouch is null)
        {
            PolymorphSpell.Logger.LogError("PlayerMovement does not have a isCrouch");
            return;
        }
        playerIsCrouch.SetValue(_playerMovement, false);

        // Update camera
        _clientPlayerCam.transform.localPosition = new Vector3(0f, 2f, -2f);
        _clientPlayerCam.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
    }

    private void OnDestroy()
    {
        PolymorphSpellData.PolymorphedPlayerNetIds.Remove(_playerNetObj.ObjectId);
        
        Destroy(_polymorphGameObject);

        // Restore pickup action
        var pickup = _player.transform.Find("pikupact");
        pickup?.gameObject.SetActive(true);

        // Restore player's skins and health
        // (player could have died while waiting for sound to finish)
        if (!_playerMovement.isDead)
        {
            // Skins
            var arms = _player.transform.Find("armz");
            arms?.gameObject.SetActive(true);
            foreach (var meshRenderer in _playerSkins)
            {
                meshRenderer.enabled = true;
            }

            // Health
            var playerHealthInfo = typeof(PlayerMovement).GetField("playerHealth", BindingFlags.NonPublic | BindingFlags.Instance);
            if (playerHealthInfo is null)
            {
                PolymorphSpell.Logger.LogError("Victim's PlayerMovement does not have a playerHealth!");
                return;
            }
            var currHealth = (float)playerHealthInfo.GetValue(_playerMovement);
            var baseline = Mathf.Min(_prePolymorphHealth, PolymorphSpellConfig.PolymorphHealth.Value);
            var damageTaken = Mathf.Max(0f, baseline - currHealth);
            var newHealth = Mathf.Clamp(_prePolymorphHealth - damageTaken, 0f, _prePolymorphHealth);
            playerHealthInfo.SetValue(_playerMovement, newHealth);
        }

        if (_isClient)
        {
            _playerMovement.ResetCam();
            
            _playerInventory.canSwapItem = true;
        }
        _polymorphAnimator = null;
        _playerMovement.canRecall = true;
        _playerMovement = null;
        _playerInventory = null;
        _clientComms.IsMuted = false;
    }

    /// <summary>
    /// Awake() that runs for all instances
    /// </summary>
    private void AwakeForAll()
    {
        _player = transform.gameObject;
        _playerMovement = GetComponent<PlayerMovement>();
        _playerNetObj = GetComponent<NetworkObject>();
        PolymorphSpellData.PolymorphedPlayerNetIds.Add(_playerNetObj.ObjectId);

        Utils.PlaySpatialSoundAtPosition(_playerMovement.transform.position, PolymorphSpellData.PolymorphCastSound);

        // Disable player skins
        _playerSkins = _playerMovement.wizardBody.Where(renderer => renderer.enabled).ToArray();
        foreach (var meshRenderer in _playerSkins)
        {
            meshRenderer.enabled = false;
        }
        var arms = _playerMovement.transform.Find("armz");
        arms?.gameObject.SetActive(false);
        var pickup = _playerMovement.transform.Find("pikupact");
        pickup?.gameObject.SetActive(false);

        // Spawn chicken and attach to victim
        _polymorphGameObject = Instantiate(PolymorphSpellData.ChickenPrefab, _playerMovement.transform.position, Quaternion.identity);
        _polymorphGameObject.transform.SetParent(_playerMovement.transform, false);
        _polymorphGameObject.transform.localPosition = Vector3.zero;
        _polymorphGameObject.transform.localRotation = Quaternion.identity;

        // Spawn chicken sounds and attach to victim
        _polymorphSoundLoop = _polymorphGameObject.AddComponent<AudioSource>();
        _polymorphSoundLoop.clip = PolymorphSpellData.ChickenSounds;
        _polymorphSoundLoop.volume = 1f;
        _polymorphSoundLoop.spatialBlend = 1f;
        _polymorphSoundLoop.rolloffMode = AudioRolloffMode.Linear;
        _polymorphSoundLoop.minDistance = 5f;
        _polymorphSoundLoop.maxDistance = 350f;
        _polymorphSoundLoop.loop = true;
        _polymorphSoundLoop.Play();

        // Spawn star explosion effects
        var starExplosionEffect = Instantiate(PolymorphSpellData.StarExplosionPrefab, _playerMovement.transform.position, Quaternion.identity);
        var effectDuration = starExplosionEffect.GetComponent<ParticleSystem>().main.duration;
        Destroy(starExplosionEffect, effectDuration);

        // Store player health and set polymorphed health
        _playerHealthField = typeof(PlayerMovement).GetField("playerHealth", BindingFlags.NonPublic | BindingFlags.Instance);
        if (_playerHealthField is null)
        {
            PolymorphSpell.Logger.LogError("PlayerMovement does not have a playerHealth!");
            return;
        }
        _prePolymorphHealth = (float)_playerHealthField.GetValue(_playerMovement);
        _playerHealthField.SetValue(_playerMovement,
            _prePolymorphHealth > PolymorphSpellConfig.PolymorphHealth.Value
                ? PolymorphSpellConfig.PolymorphHealth.Value
                : _prePolymorphHealth);
    }

    /// <summary>
    /// Awake() but runs only if this instance is the local client
    /// </summary>
    private void AwakeForLocal()
    {
        _isClient = true;
        
        _playerInventory = GetComponent<PlayerInventory>();
        if (_playerInventory is null)
        {
            PolymorphSpell.Logger.LogError("PlayerInventory is null!");
            return;
        }

        _playerInventory.canSwapItem = false;

        _polymorphAnimator = _polymorphGameObject.GetComponent<Animator>();
        if (_polymorphAnimator is null)
        {
            PolymorphSpell.Logger.LogError("Polymorph animator not found!");
            return;
        }

        var playerCamInfo = typeof(PlayerMovement).GetField("playerCamera", BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerCamInfo is null)
        {
            PolymorphSpell.Logger.LogError("PlayerMovement does not have a playerCamera");
            return;
        }
        _clientPlayerCam = (Camera)playerCamInfo.GetValue(_playerMovement);
        if (_clientPlayerCam is null)
        {
            PolymorphSpell.Logger.LogError("Client PlayerMovement's playerCamera is null!");
            return;
        }

        _clientArmsAni = _playerMovement.ArmsAni;
        if (_clientArmsAni is null)
        {
            PolymorphSpell.Logger.LogError("Client PlayerMovement's ArmsAni is null!");
            return;
        }

        _clientComms = FindFirstObjectByType<DissonanceComms>();
        if (_clientComms is null)
        {
            PolymorphSpell.Logger.LogError("Client PlayerMovement's ArmsAni is null!");
            return;
        }
        _clientComms.IsMuted = true;
    }
}
