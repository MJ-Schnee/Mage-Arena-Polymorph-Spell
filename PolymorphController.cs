using System.Reflection;
using UnityEngine;

namespace PolymorphSpell;

/// <summary>
/// Camera controller attached to player when polymorphed
/// </summary>
internal class PolymorphController: MonoBehaviour
{
    internal static bool IsClientPolymorphed { get; set; }
    
    internal static PlayerMovement ClientPlayerMovement { get; set; }
    
    internal static Animator ClientPolymorphAnimator { get; set; }
    
    private readonly int _polymorphAnimatorWalkingId = Animator.StringToHash("Vert");
    
    private readonly int _polymorphAnimatorRunningId = Animator.StringToHash("State");

    private Camera _clientPlayerCam;

    private Animator _clientArmsAni;
    
    private void Awake()
    {
        var playerCamInfo = typeof(PlayerMovement).GetField("playerCamera", BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerCamInfo is null)
        {
            PolymorphSpell.Logger.LogError("PlayerMovement does not have a playerCamera");
            return;
        }
        _clientPlayerCam = (Camera)playerCamInfo.GetValue(ClientPlayerMovement);
        if (_clientPlayerCam is null)
        {
            PolymorphSpell.Logger.LogError("Client PlayerMovement's playerCamera is null!");
            return;
        }
        
        _clientArmsAni = ClientPlayerMovement.ArmsAni;
        if (_clientArmsAni is null)
            PolymorphSpell.Logger.LogError("Client PlayerMovement's ArmsAni is null!");
    }
    
    private void Update()
    {
        if (!IsClientPolymorphed)
            return;
        
        // Disable recall
        ClientPlayerMovement.canRecall = false;
        
        // Disable crouch
        var playerIsCrouch = typeof(PlayerMovement).GetField("isCrouch", BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerIsCrouch is null)
        {
            PolymorphSpell.Logger.LogError("PlayerMovement does not have a isCrouch");
            return;
        }
        playerIsCrouch.SetValue(ClientPlayerMovement, false);
        
        // Update camera
        _clientPlayerCam.transform.localPosition = new Vector3(0f, 2f, -2f);
        _clientPlayerCam.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

        // Update polymorph animator
        ClientPolymorphAnimator.SetFloat(_polymorphAnimatorWalkingId,
            ClientPlayerMovement.currentSpeed / ClientPlayerMovement.walkingSpeed);
        ClientPolymorphAnimator.SetFloat(_polymorphAnimatorRunningId,
            ClientPlayerMovement.currentSpeed > ClientPlayerMovement.walkingSpeed
                ? ClientPlayerMovement.currentSpeed / ClientPlayerMovement.runningSpeed
                : 0f);
    }

    private void OnDestroy()
    {
        ClientPlayerMovement.canRecall = true;
    }
}