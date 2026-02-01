using UnityEngine;
using UnityEngine.InputSystem;
using System; // Fondamentale per Action
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
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
    [SerializeField] private Transform view;

    private CharacterController controller;
    private Vector3 velocity;
    private float yaw;
    private float pitch;
    private int iJumpsRemaining;
    private CameraShake cameraShakeScript;
    
    // Respawn System
    private Vector3 currentRespawnPoint;

    // Oni Runtime
    private float fOniHoldShiftTime;
    private float fOniCurrentMultiplier = 1f;
    private bool bIsStunned = false; // Impedisce l'input durante il rimbalzo

    // Yuki Runtime
    private bool bYukiSecondJumpAvailable;

    private MaskAbilityConfig activeMaskConfig;
    private float fDefaultMoveSpeed, fDefaultJumpForce, fDefaultGroundFriction, fDefaultAirFriction, fDefaultGravity;
    private int iDefaultMaxJumps;

    // ------------------------------------------------------------------
    // MODIFICHE QUI SOTTO (Eventi e Getter Pubblici corretti)
    // ------------------------------------------------------------------

    // Audio Managment
    // Corretto: Action<bool> accetta il parametro true/false per il doppio salto
    public event Action<bool> OnJump; 
    public event Action<MaskType> OnMaskChanged;

    // Getter Pubblici (Rinomimati per combaciare con PlayerAudioController)
    // Ho tolto i prefissi 'b' e 'f' dai nomi pubblici per risolvere gli errori CS1061
    public bool IsGrounded => controller.isGrounded;
    public float CurrentSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
    public bool IsOniSprinting => fOniCurrentMultiplier > 1.1f;
    public MaskType CurrentMask => activeMaskConfig != null ? activeMaskConfig.maskType : MaskType.Default;

    // ------------------------------------------------------------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (view != null) cameraShakeScript = view.GetComponentInChildren<CameraShake>();

        fDefaultMoveSpeed = fMoveSpeed;
        fDefaultJumpForce = fJumpForce;
        iDefaultMaxJumps = iMaxJumps;
        fDefaultGroundFriction = fGroundFriction;
        fDefaultAirFriction = fAirFriction;
        fDefaultGravity = fGravity;
        
        iJumpsRemaining = iMaxJumps;
        currentRespawnPoint = transform.position; // Inizializzazione sicurezza
        ApplyCursorState();
    }

    private void Update()
    {
        if (!bIsGameplay) return;

        // Se siamo storditi dal rimbalzo, saltiamo l'input ma continuiamo a muovere per la fisica
        if (!bIsStunned)
        {
            HandleMovement();
            HandleJump();
            HandleLook();
        }

        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeMaskConfig == null) return;

        // --- GESTIONE MURI ---
        if (other.CompareTag(activeMaskConfig.wallTag))
        {
            float currentThreshold = fOniCurrentMultiplier / activeMaskConfig.fOniMaxSpeedMultiplier;

            if (activeMaskConfig.maskType == MaskType.Oni)
            {
                if (currentThreshold >= activeMaskConfig.fOniMaxSpeedThreshold01)
                {
                    Debug.Log("Oni sfonda il muro!");
                    SetDefaultStats();
                }
                else
                {
                    ApplyBounceImpact(other);
                }
            }
            else if (activeMaskConfig.maskType == MaskType.Yuki)
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

        float bounceForce = fMoveSpeed * 1.5f;
        velocity = bounceDirection * bounceForce;
        velocity.y = fJumpForce * 0.4f;

        if (cameraShakeScript != null)
        {
            cameraShakeScript.TriggerShake(0.4f, 0.6f);
        }

        // Avviamo una coroutine per ripristinare il controllo dopo l'urto
        StartCoroutine(StunRoutine(0.3f));
    }

    private IEnumerator StunRoutine(float duration)
    {
        bIsStunned = true;
        yield return new WaitForSeconds(duration);
        bIsStunned = false;
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
        }

        Vector3 moveDir = transform.forward * input.y + transform.right * input.x;
        moveDir.y = 0;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        float targetSpeed = fMoveSpeed;
        float currentControl = controller.isGrounded ? fGroundFriction : fAirFriction;

        // Sprint SOLO Oni
        bool bShiftHeld = kb != null && kb.leftShiftKey.isPressed;
        if (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Oni && bShiftHeld && input.sqrMagnitude > 0.01f)
        {
            fOniHoldShiftTime += Time.deltaTime;
            if (fOniHoldShiftTime >= activeMaskConfig.fOniChargeDelay)
            {
                fOniCurrentMultiplier = Mathf.MoveTowards(fOniCurrentMultiplier, activeMaskConfig.fOniMaxSpeedMultiplier, activeMaskConfig.fOniChargeRate * Time.deltaTime);
            }
        }
        else
        {
            fOniHoldShiftTime = 0f;
            fOniCurrentMultiplier = Mathf.MoveTowards(fOniCurrentMultiplier, 1f, (activeMaskConfig != null ? activeMaskConfig.fOniDissipateRate : 5f) * Time.deltaTime);
        }

        targetSpeed *= fOniCurrentMultiplier;
        Vector3 targetVelocity = moveDir * targetSpeed;
        float t = 1f - Mathf.Exp(-currentControl * Time.deltaTime);
        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, t);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, t);
    }

    private void HandleJump()
    {
        if (controller.isGrounded)
        {
            iJumpsRemaining = iMaxJumps;
            bYukiSecondJumpAvailable = (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Yuki);
            if (velocity.y < 0) velocity.y = -2f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            bool jumpSuccess = false;
            bool isDoubleJump = false;

            if (controller.isGrounded)
            {
                velocity.y = fJumpForce;
                iJumpsRemaining--;
                jumpSuccess = true;
                isDoubleJump = false;

                if (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Oni && fOniCurrentMultiplier > 1.1f)
                {
                    velocity.x *= activeMaskConfig.fOniMomentumJumpMultiplier;
                    velocity.z *= activeMaskConfig.fOniMomentumJumpMultiplier;
                }
            }
            else if (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Yuki && bYukiSecondJumpAvailable)
            {
                velocity.y = activeMaskConfig.fYukiSecondJumpForce;
                bYukiSecondJumpAvailable = false;
                jumpSuccess = true;
                isDoubleJump = true;
            }

            if (jumpSuccess) OnJump?.Invoke(isDoubleJump);
        }
    }

    private void ApplyGravity() => velocity.y += fGravity * Time.deltaTime;

    // --- RESPAWN & CHECKPOINT API ---
    public void SetRespawnPoint(Vector3 newPos) => currentRespawnPoint = newPos;

    public void TeleportTo(Vector3 pos) 
    { 
        controller.enabled = false; 
        transform.position = pos; 
        controller.enabled = true; 
        velocity = Vector3.zero; 
    }

    // --- MASK API ---
    public void ApplyMask(MaskAbilityConfig cfg)
    {
        activeMaskConfig = cfg;
        SetDefaultStats();
        
        // CORREZIONE: Qui applichiamo anche l'evento cambio maschera
        if (cfg == null) return;
        if (cfg.moveSpeed > 0) fMoveSpeed = cfg.moveSpeed;
        if (cfg.jumpForce > 0) fJumpForce = cfg.jumpForce;
        if (cfg.maxJumps >= 0) iMaxJumps = cfg.maxJumps;
        iJumpsRemaining = iMaxJumps;

        // Notifica il cambio maschera all'audio
        OnMaskChanged?.Invoke(cfg.maskType);
    }

    public void SetDefaultStats()
    {
        fMoveSpeed = fDefaultMoveSpeed;
        fJumpForce = fDefaultJumpForce;
        iMaxJumps = iDefaultMaxJumps;
        fOniCurrentMultiplier = 1f;
        fOniHoldShiftTime = 0f;
    }

    public void SetGameplayControl(bool active) { bIsGameplay = active; ApplyCursorState(); }

    private void HandleLook()
    {
        if (Mouse.current == null || view == null) return;
        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * 0.1f;
        pitch = Mathf.Clamp(pitch - delta.y * 0.1f, fPitchMin, fPitchMax);
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        view.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void ApplyCursorState() { Cursor.lockState = bIsGameplay ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !bIsGameplay; }
}