using UnityEngine;
using UnityEngine.InputSystem;
using System; 
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    // ==========================================
    // PRIVATE VARIABLES
    // ==========================================
    [Header("Input Setup")]
    [SerializeField] private InputActionAsset iaInputAsset;

    [Header("Look")] 
    [SerializeField] private float fPitchMin = -80f;
    [SerializeField] private float fPitchMax = 80f;
    [SerializeField] private bool bIsGameplay = true;

    [Header("Movement (Base)")] 
    [SerializeField] private float fMoveSpeed = 6f;

    [Header("Jump (Base)")] 
    [SerializeField] private float fJumpForce = 8f;

    [SerializeField] private int iMaxJumps = 1;

    [Header("Gravity (Base)")] 
    [SerializeField] private float fGravity = -25f;

    [Header("Friction (Base)")] 
    [SerializeField] private float fGroundFriction = 12f;

    [SerializeField] private float fAirFriction = 3f;

    [Header("References")] 
    [SerializeField] private Transform tView;

    private CharacterController cController;
    private Vector3 velocity;
    private float fYaw;
    private float fPitch;
    private int iJumpsRemaining;
    private CameraShake csCameraShakeScript;

    // Respawn System
    private Vector3 currentRespawnPoint;
    private Quaternion qCurrentRespawnRotation;

    // Oni Runtime
    private float fOniHoldShiftTime;
    private float fOniCurrentMultiplier = 1f;
    private bool bIsStunned = false; 

    // Yuki Runtime
    private bool bYukiSecondJumpAvailable;

    private MaskAbilityConfig macActiveMaskConfig;
    private float fDefaultMoveSpeed, fDefaultJumpForce, fDefaultGroundFriction, fDefaultAirFriction, fDefaultGravity;
    private int iDefaultMaxJumps;
    
    private Transform tActivePlatform;
    private Vector3 lastPlatformPosition;

    private InputActionMap iamPlayerMap;
    private InputAction iaMoveAction;
    private InputAction iaLookAction;
    private InputAction iaJumpAction;
    private InputAction iaSprintAction;

    // ==========================================
    // PUBLIC VARIABLES E EVENTI
    // ==========================================
    public event Action<bool> OnJump;
    public event Action<MaskType> OnMaskChanged;

    public bool IsGrounded => cController != null && cController.isGrounded;
    public float CurrentSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
    public bool IsOniSprinting => fOniCurrentMultiplier > 1.1f;
    public MaskType CurrentMask => macActiveMaskConfig != null ? macActiveMaskConfig.maskType : MaskType.Default;

    // ==========================================
    // PRIVATE METHODS
    // ==========================================
    private void Awake()
    {
        cController = GetComponent<CharacterController>();
        if (tView != null) csCameraShakeScript = tView.GetComponentInChildren<CameraShake>();

        // FIX INCLINAZIONE: Assicuriamoci che il player sia dritto se lo spawn point era storto
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        fYaw = transform.eulerAngles.y;

        fDefaultMoveSpeed = fMoveSpeed;
        fDefaultJumpForce = fJumpForce;
        iDefaultMaxJumps = iMaxJumps;
        fDefaultGroundFriction = fGroundFriction;
        fDefaultAirFriction = fAirFriction;
        fDefaultGravity = fGravity;

        iJumpsRemaining = iMaxJumps;
        currentRespawnPoint = transform.position; 
        
        SetupInputs();
        ApplyCursorState();
    }

    private void SetupInputs()
    {
        if (iaInputAsset == null) return;

        iamPlayerMap = iaInputAsset.FindActionMap("Player");
        if (iamPlayerMap != null)
        {
            // FIX MOVIMENTO: Abilitiamo forzatamente l'intera mappa per gestire il cambio periferiche
            iamPlayerMap.Enable(); 
            iaMoveAction = iamPlayerMap.FindAction("Move");
            iaLookAction = iamPlayerMap.FindAction("Look");
            iaJumpAction = iamPlayerMap.FindAction("Jump");
            iaSprintAction = iamPlayerMap.FindAction("Sprint");
        }
    }

    private void OnEnable() => iamPlayerMap?.Enable();
    private void OnDisable() => iamPlayerMap?.Disable();

    private void Update()
    {
        if (!bIsGameplay) return;
        
        HandleMovingPlatform();

        if (!bIsStunned)
        {
            HandleMovement();
            HandleJump();
            HandleLook();
        }

        ApplyGravity();
        cController.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (macActiveMaskConfig == null) return;

        if (other.CompareTag(macActiveMaskConfig.wallTag))
        {
            float fCurrentThreshold = fOniCurrentMultiplier / macActiveMaskConfig.fOniMaxSpeedMultiplier;

            if (macActiveMaskConfig.maskType == MaskType.Oni)
            {
                if (fCurrentThreshold >= macActiveMaskConfig.fOniMaxSpeedThreshold01)
                {
                    SetDefaultStats();
                }
                else
                {
                    ApplyBounceImpact(other);
                }
            }
            else if (macActiveMaskConfig.maskType == MaskType.Yuki)
            {
                ApplyBounceImpact(other);
            }
        }
    }

    private void ApplyBounceImpact(Collider wall)
    {
        if (bIsStunned) return;

        SetDefaultStats();

        Vector3 bounceDirection = (transform.position - wall.transform.position).normalized;
        bounceDirection.y = 0;

        float fBounceForce = fMoveSpeed * 3.0f;
        velocity = bounceDirection * fBounceForce;
        velocity.y = fJumpForce * 0.4f;

        if (csCameraShakeScript != null) csCameraShakeScript.TriggerShake(0.4f, 0.6f);

        StartCoroutine(StunRoutine(0.3f));
    }

    private IEnumerator StunRoutine(float fDuration)
    {
        bIsStunned = true;
        yield return new WaitForSeconds(fDuration);
        bIsStunned = false;
    }

    private void HandleMovement()
    {
        Vector2 input = iaMoveAction != null ? iaMoveAction.ReadValue<Vector2>() : Vector2.zero;

        Vector3 moveDir = transform.forward * input.y + transform.right * input.x;
        moveDir.y = 0;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        float fTargetSpeed = fMoveSpeed;
        float fCurrentControl = cController.isGrounded ? fGroundFriction : fAirFriction;

        bool bSprintHeld = iaSprintAction != null && iaSprintAction.IsPressed();

        if (macActiveMaskConfig != null && macActiveMaskConfig.maskType == MaskType.Oni && bSprintHeld &&
            input.sqrMagnitude > 0.01f)
        {
            fOniHoldShiftTime += Time.deltaTime;
            if (fOniHoldShiftTime >= macActiveMaskConfig.fOniChargeDelay)
            {
                fOniCurrentMultiplier = Mathf.MoveTowards(fOniCurrentMultiplier,
                    macActiveMaskConfig.fOniMaxSpeedMultiplier, macActiveMaskConfig.fOniChargeRate * Time.deltaTime);
            }
        }
        else
        {
            fOniHoldShiftTime = 0f;
            fOniCurrentMultiplier = Mathf.MoveTowards(fOniCurrentMultiplier, 1f,
                (macActiveMaskConfig != null ? macActiveMaskConfig.fOniDissipateRate : 5f) * Time.deltaTime);
        }

        fTargetSpeed *= fOniCurrentMultiplier;
        Vector3 targetVelocity = moveDir * fTargetSpeed;
        float fTimeParam = 1f - Mathf.Exp(-fCurrentControl * Time.deltaTime);
        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, fTimeParam);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, fTimeParam);
    }

    private void HandleJump()
    {
        if (cController.isGrounded)
        {
            iJumpsRemaining = iMaxJumps;
            bYukiSecondJumpAvailable = (macActiveMaskConfig != null && macActiveMaskConfig.maskType == MaskType.Yuki);
            if (velocity.y < 0) velocity.y = -2f;
        }

        bool bJumpPressed = iaJumpAction != null && iaJumpAction.WasPressedThisFrame();

        if (bJumpPressed)
        {
            bool bJumpSuccess = false;
            bool bIsDoubleJump = false;

            if (cController.isGrounded)
            {
                velocity.y = fJumpForce;
                iJumpsRemaining--;
                bJumpSuccess = true;
                bIsDoubleJump = false;

                if (macActiveMaskConfig != null && macActiveMaskConfig.maskType == MaskType.Oni &&
                    fOniCurrentMultiplier > 1.1f)
                {
                    velocity.x *= macActiveMaskConfig.fOniMomentumJumpMultiplier;
                    velocity.z *= macActiveMaskConfig.fOniMomentumJumpMultiplier;
                }
            }
            else if (macActiveMaskConfig != null && macActiveMaskConfig.maskType == MaskType.Yuki && bYukiSecondJumpAvailable)
            {
                velocity.y = macActiveMaskConfig.fYukiSecondJumpForce;
                bYukiSecondJumpAvailable = false;
                bJumpSuccess = true;
                bIsDoubleJump = true;
            }

            if (bJumpSuccess) OnJump?.Invoke(bIsDoubleJump);
        }
    }

    private void ApplyGravity() => velocity.y += fGravity * Time.deltaTime;

    private void HandleMovingPlatform()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.4f))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                if (tActivePlatform == hit.collider.transform)
                {
                    Vector3 platformDelta = tActivePlatform.position - lastPlatformPosition;
                    if (platformDelta != Vector3.zero) cController.Move(platformDelta);
                }
                tActivePlatform = hit.collider.transform;
                lastPlatformPosition = tActivePlatform.position;
                return;
            }
        }
        tActivePlatform = null;
    }

    private void HandleLook()
    {
        if (iaLookAction == null || tView == null || Time.deltaTime == 0f) return;

        Vector2 lookInput = iaLookAction.ReadValue<Vector2>();
        if (lookInput == Vector2.zero) return;

        float fSensitivity = MouseSensitivityController.fMouseSensitivity > 0 ? MouseSensitivityController.fMouseSensitivity : 0.16f;

        // FIX ROTAZIONE TELECAMERA: Riconosce se usi mouse o gamepad e adatta i valori
        bool bIsGamepad = iaLookAction.activeControl?.device is Gamepad;
        float fInputMultiplier = bIsGamepad ? 150f * Time.deltaTime : 1f;
        
        fYaw += lookInput.x * fSensitivity * fInputMultiplier;
        fPitch = Mathf.Clamp(fPitch - lookInput.y * fSensitivity * fInputMultiplier, fPitchMin, fPitchMax);
        
        transform.rotation = Quaternion.Euler(0, fYaw, 0);
        tView.localRotation = Quaternion.Euler(fPitch, 0, 0);
    }

    private void ApplyCursorState()
    {
        Cursor.lockState = bIsGameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bIsGameplay;
    }

    // ==========================================
    // PUBLIC METHODS
    // ==========================================
    public void SetRespawnPoint(Vector3 newPos) => currentRespawnPoint = newPos;
    
    public void SetRespawnPoint(Vector3 newPos, Quaternion qNewRot) 
    {
        currentRespawnPoint = newPos;
        qCurrentRespawnRotation = qNewRot;
    }

    public void TeleportTo(Vector3 pos)
    {
        cController.enabled = false;
        transform.position = pos;
        cController.enabled = true;
        velocity = Vector3.zero;
    }

    public void TeleportTo(Vector3 pos, Quaternion qRot)
    {
        cController.enabled = false;
        transform.position = pos;
        fYaw = qRot.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, fYaw, 0);
        fPitch = 0; 
        if (tView != null) tView.localRotation = Quaternion.Euler(0, 0, 0);
        cController.enabled = true;
        velocity = Vector3.zero;
    }
    
    public void ApplyMask(MaskAbilityConfig macCfg)
    {
        macActiveMaskConfig = macCfg;
        SetDefaultStats();

        if (macCfg == null) return;
        if (macCfg.moveSpeed > 0) fMoveSpeed = macCfg.moveSpeed;
        if (macCfg.jumpForce > 0) fJumpForce = macCfg.jumpForce;
        if (macCfg.maxJumps >= 0) iMaxJumps = macCfg.maxJumps;
        iJumpsRemaining = iMaxJumps;

        OnMaskChanged?.Invoke(macCfg.maskType);
    }

    public void SetDefaultStats()
    {
        fMoveSpeed = fDefaultMoveSpeed;
        fJumpForce = fDefaultJumpForce;
        iMaxJumps = iDefaultMaxJumps;
        fOniCurrentMultiplier = 1f;
        fOniHoldShiftTime = 0f;
    }

    public void SetGameplayControl(bool bActive)
    {
        bIsGameplay = bActive;
        ApplyCursorState();
    }
    
    public void HardStop()
    {
        velocity = Vector3.zero;
        fOniCurrentMultiplier = 1f;
        fOniHoldShiftTime = 0f;
        tActivePlatform = null;
        lastPlatformPosition = Vector3.zero;
    }
}