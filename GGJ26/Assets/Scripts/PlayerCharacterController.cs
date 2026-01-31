using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private float fJumpForce = 7f;
    [SerializeField] private float iMaxJumps = 1;
    


    [Header("Dash (Base)")]
    [SerializeField] private float fDashSpeed = 18f;
    [SerializeField] private float fDashCooldown = 0.5f;
    [SerializeField] private bool bDashOnlyWhenGrounded = false;

    [Header("Gravity (Base)")]
    [SerializeField] private float fGravity = -20f;

    [Header("Friction (Base)")]
    [Tooltip("Più alto = più controllo a terra (meno slittamento). Più basso = più inerzia/slittamento.")]
    [SerializeField] private float fGroundFriction = 10f;

    [Tooltip("Più alto = più controllo in aria. Più basso = più 'floaty' e inerziale.")]
    [SerializeField] private float fAirFriction = 2.5f;

    [Header("References")]
    [SerializeField] private Transform view; // Camera o pivot (pitch)

    // -------------------------------------------------------
    // Runtime
    // -------------------------------------------------------
    private CharacterController controller;
    private Vector3 velocity;

    private float yaw;
    private float pitch;

    private int iJumpsRemaining;
    private float fLastDashTime;

    private Vector3 currentRespawnPoint;

    // -------------------------------------------------------
    // Mask system runtime
    // -------------------------------------------------------
    [Header("Mask Runtime (Read Only)")]
    [SerializeField] private MaskType currentMaskType = MaskType.Default;
    [SerializeField] private bool bIsOniInfiniteDashActive;
    [SerializeField] private bool bIsStunned;

    private MaskAbilityConfig activeMaskConfig;

    // Defaults saved on Awake
    private float fDefaultMoveSpeed;
    private float fDefaultJumpForce;
    private float fDefaultMaxJumps;
    private float fDefaultDashSpeed;
    private float fDefaultDashCooldown;
    private float fDefaultGravity;
    private float fDefaultGroundFriction;
    private float fDefaultAirFriction;

    // Coroutines references (evitiamo StopAllCoroutines)
    private Coroutine corYukiZeroGCoroutine;
    private Coroutine corOniResetCoroutine;
    private Coroutine corOniStunCoroutine;

    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Save defaults
        fDefaultMoveSpeed = fMoveSpeed;
        fDefaultJumpForce = fJumpForce;
        fDefaultMaxJumps = iMaxJumps;
        fDefaultDashSpeed = fDashSpeed;
        fDefaultDashCooldown = fDashCooldown;
        fDefaultGravity = fGravity;
        fDefaultGroundFriction = fGroundFriction;
        fDefaultAirFriction = fAirFriction;

        iJumpsRemaining = (int)iMaxJumps;

        yaw = transform.eulerAngles.y;
        pitch = (view != null) ? view.localEulerAngles.x : transform.eulerAngles.x;

        ApplyCursorState();
    }

    private void Update()
    {
        HandleOniSpaceReset(); // se Oni infinite dash e premi SPACE -> reset+recovery

        HandleMovement_NewInputSystem();
        HandleJump_NewInputSystem();
        HandleDash_NewInputSystem();

        ApplyGravity();
        HandleLook();

        controller.Move(velocity * Time.deltaTime);
    }

    // -------------------------------------------------------
    // MOVEMENT
    // -------------------------------------------------------
    private void HandleMovement_NewInputSystem()
    {
        Vector2 input = Vector2.zero;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
        }

        Vector3 forward = (view != null) ? view.forward : transform.forward;
        Vector3 right = (view != null) ? view.right : transform.right;

        Vector3 moveDir = forward * input.y + right * input.x;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Target velocity (orizzontale)
        Vector3 target = moveDir * fMoveSpeed;

        // Friction/control smoothing:
        // - higher => high "control" (less sliding)
        // - lower => less control (more sliding)
        float control = controller.isGrounded ? fGroundFriction : fAirFriction;
        float t = 1f - Mathf.Exp(-control * Time.deltaTime);

        // If Oni is on InfiniteDash it have to be tuned by dash handler
        if (!bIsOniInfiniteDashActive && !bIsStunned)
        {
            velocity.x = Mathf.Lerp(velocity.x, target.x, t);
            velocity.z = Mathf.Lerp(velocity.z, target.z, t);
        }
    }

    // -------------------------------------------------------
    // JUMP
    // -------------------------------------------------------
    private void HandleJump_NewInputSystem()
    {
        // Reset jump counter when grounded
        if (controller.isGrounded)
        {
            iJumpsRemaining = (int)iMaxJumps;

            // Keep grounded stability
            if (velocity.y < 0f)
                velocity.y = -2f;
        }

        bool bJumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (!bJumpPressed || iJumpsRemaining <= 0 || bIsStunned)
            return;

        // If Oni infinite dash is active, stop it when jumping
        if (bIsOniInfiniteDashActive)
            bIsOniInfiniteDashActive = false;

        float fFinalJumpForce = ComputeFinalJumpForce();
        velocity.y = fFinalJumpForce;

        iJumpsRemaining--;
    }

    private float ComputeFinalJumpForce()
    {
        float fFinalJumpForce = fJumpForce;

        // Oni: jump scales with horizontal speed
        if (activeMaskConfig != null &&
            activeMaskConfig.maskType == MaskType.Oni &&
            activeMaskConfig.bOniJumpScaleWithSpeed)
        {
            float fSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            float fMaxSpeed = Mathf.Max(0.01f, activeMaskConfig.fOniSpeedForMaxJump);

            float t = Mathf.Clamp01(fSpeed / fMaxSpeed);
            float fMultiplier = Mathf.Lerp(
                activeMaskConfig.fOniJumpMinMultiplier,
                activeMaskConfig.fOniJumpMaxMultiplier,
                t
            );

            fFinalJumpForce *= fMultiplier;
        }

        return fFinalJumpForce;
    }


    // -------------------------------------------------------
    // DASH
    // -------------------------------------------------------
    private void HandleDash_NewInputSystem()
    {
        if (bIsStunned) return;

        // Oni infinite dash: obligate forward force
        // and it works only if is pressing
        // Oni infinite dash: active ONLY while holding Shift
        if (bIsOniInfiniteDashActive)
        {
            bool bShiftHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            // If shift is not held, do not force dash
            if (!bShiftHeld)
                return;

            Vector3 vDashDir = (view != null) ? view.forward : transform.forward;
            vDashDir.y = 0f;
            vDashDir.Normalize();

            velocity.x = vDashDir.x * fDashSpeed;
            velocity.z = vDashDir.z * fDashSpeed;
            return;
        }



        if (Time.time < fLastDashTime + fDashCooldown)
            return;

        if (bDashOnlyWhenGrounded && !controller.isGrounded)
            return;

        bool dashPressed = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;

        if (dashPressed)
        {
            Vector3 dashDir = (view != null) ? view.forward : transform.forward;
            dashDir.y = 0f;
            dashDir.Normalize();

            // OVERRIDE: no vectorial sum
            velocity.x = dashDir.x * fDashSpeed;
            velocity.z = dashDir.z * fDashSpeed;

            fLastDashTime = Time.time;
        }
    }

    // -------------------------------------------------------
    // GRAVITY
    // -------------------------------------------------------
    private void ApplyGravity()
    {
        //fGravity set at 0 temporary if Yuki have zero gravity activated
        velocity.y += fGravity * Time.deltaTime;
    }

    // -------------------------------------------------------
    // LOOK
    // -------------------------------------------------------
    private void HandleLook()
    {
        if (!bIsGameplay) return;
        if (view == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * MouseSensitivityController.fMouseSensitivity;
        pitch -= delta.y * MouseSensitivityController.fMouseSensitivity;

        pitch = Mathf.Clamp(pitch, fPitchMin, fPitchMax);

        // Yaw on player
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        // Pitch on pivot/camera
        view.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void ApplyCursorState()
    {
        Cursor.lockState = bIsGameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bIsGameplay;
    }

    // -------------------------------------------------------
    // MASK API (Designer Friendly)
    // -------------------------------------------------------
    public void ApplyMask(MaskAbilityConfig cfg)
    {
        if (cfg == null)
        {
            SetDefaultStats();
            return;
        }

        activeMaskConfig = cfg;
        currentMaskType = cfg.maskType;

        // Stop only relevant running routines
        if (corYukiZeroGCoroutine != null) { StopCoroutine(corYukiZeroGCoroutine); corYukiZeroGCoroutine = null; }
        if (corOniResetCoroutine != null) { StopCoroutine(corOniResetCoroutine); corOniResetCoroutine = null; }
        if (corOniStunCoroutine != null) { StopCoroutine(corOniStunCoroutine); corOniStunCoroutine = null; }

        // Reset to defaults first
        SetDefaultStats();

        // Apply overrides (common)
        if (cfg.moveSpeed >= 0f) fMoveSpeed = cfg.moveSpeed;
        if (cfg.jumpForce >= 0f) fJumpForce = cfg.jumpForce;
        if (cfg.maxJumps >= 0) iMaxJumps = cfg.maxJumps;

        if (cfg.dashSpeed >= 0f) fDashSpeed = cfg.dashSpeed;
        if (cfg.dashCooldown >= 0f) fDashCooldown = cfg.dashCooldown;

        if (cfg.airFriction >= 0f) fAirFriction = cfg.airFriction;
        if (cfg.groundFriction >= 0f) fGroundFriction = cfg.groundFriction;

        if (!Mathf.Approximately(cfg.gravity, -9999f)) fGravity = cfg.gravity;

        // Apply abilities
        if (cfg.maskType == MaskType.Yuki)
        {
            // Yuki: improve jump (via override)and air friction (override),
            // zero gravity for N seconds (temp fly)
            if (cfg.yukiEnableZeroGravity)
                corYukiZeroGCoroutine = StartCoroutine(YukiZeroGravityRoutine(cfg.yukiZeroGravityDuration));
        }
        else if (cfg.maskType == MaskType.Oni)
        {
            // Oni: infinite dash until SPACE
            if (cfg.oniInfiniteDash)
                bIsOniInfiniteDashActive = true;
        }

        // Ensure jumps remaining respects new max
        iJumpsRemaining = (int)Mathf.Clamp(iJumpsRemaining, 0, iMaxJumps);
    }

    public void SetDefaultStats()
    {
        fMoveSpeed = fDefaultMoveSpeed;
        fJumpForce = fDefaultJumpForce;
        iMaxJumps = fDefaultMaxJumps;

        fDashSpeed = fDefaultDashSpeed;
        fDashCooldown = fDefaultDashCooldown;

        fGravity = fDefaultGravity;
        fGroundFriction = fDefaultGroundFriction;
        fAirFriction = fDefaultAirFriction;

        bIsOniInfiniteDashActive = false;
        bIsStunned = false;
    }

    private IEnumerator YukiZeroGravityRoutine(float seconds)
    {
        float oldGravity = fGravity;
        fGravity = 0f;

        yield return new WaitForSeconds(Mathf.Max(0.01f, seconds));

        // Reset only if Yuki is activated
        if (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Yuki)
            fGravity = oldGravity;

        corYukiZeroGCoroutine = null;
    }

    // Oni: press SPACE during infinite dash => reset after delay + incremental recovery 
    private void HandleOniSpaceReset()
    {
        if (activeMaskConfig == null) return;
        if (activeMaskConfig.maskType != MaskType.Oni) return;
        if (!bIsOniInfiniteDashActive) return;

        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (!spacePressed) return;

        // Shut infinite dash down and avvia reset
        bIsOniInfiniteDashActive = false;

        if (corOniResetCoroutine != null) StopCoroutine(corOniResetCoroutine);
        corOniResetCoroutine = StartCoroutine(OniResetAndRecoverRoutine());
    }

    private IEnumerator OniResetAndRecoverRoutine()
    {
        float delay = Mathf.Max(0f, activeMaskConfig.oniResetDelayAfterSpace);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // Reset a default
        SetDefaultStats();

        // Recovery with running
        float duration = Mathf.Max(0.01f, activeMaskConfig.oniRecoveryDuration);

        float t = 0f;
        float startMove = fMoveSpeed;   // default
        float startDash = fDashSpeed;   // default
        
        // Re-increment with Oni values only during move.
        float targetMove = (activeMaskConfig.moveSpeed >= 0f) ? activeMaskConfig.moveSpeed : fDefaultMoveSpeed;
        float targetDash = (activeMaskConfig.dashSpeed >= 0f) ? activeMaskConfig.dashSpeed : fDefaultDashSpeed;

        // Ground friction "slip a bit"
        float targetGroundFriction = (activeMaskConfig.groundFriction >= 0f) ? activeMaskConfig.groundFriction : fDefaultGroundFriction;

        while (t < duration)
        {
            bool moving =
                Keyboard.current != null &&
                (Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed ||
                 Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed);

            if (moving)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);

                fMoveSpeed = Mathf.Lerp(startMove, targetMove, k);
                fDashSpeed = Mathf.Lerp(startDash, targetDash, k);
                fGroundFriction = Mathf.Lerp(fDefaultGroundFriction, targetGroundFriction, k);
            }

            yield return null;
        }

        //After reset return default, then recovery values.
        //Check if return infinite dash after the recovery                                          <==================TODO
        if (activeMaskConfig != null && activeMaskConfig.maskType == MaskType.Oni)
        {
            // Override without re-enable infinite dash:
            float savedGravity = fGravity;
            float savedAir = fAirFriction;

            ApplyMask_NoEnableInfiniteDash(activeMaskConfig);

            fGravity = savedGravity;
            fAirFriction = savedAir;
        }

        corOniResetCoroutine = null;
    }

    private void ApplyMask_NoEnableInfiniteDash(MaskAbilityConfig cfg)
    {
        // Variante interna: come ApplyMask, ma NON attiva infinite dash.
        activeMaskConfig = cfg;
        currentMaskType = cfg.maskType;

        // non stoppo coroutine qui (la stiamo chiamando dentro la coroutine)
        SetDefaultStats();

        if (cfg.moveSpeed >= 0f) fMoveSpeed = cfg.moveSpeed;
        if (cfg.jumpForce >= 0f) fJumpForce = cfg.jumpForce;
        if (cfg.maxJumps >= 0) iMaxJumps = cfg.maxJumps;

        if (cfg.dashSpeed >= 0f) fDashSpeed = cfg.dashSpeed;
        if (cfg.dashCooldown >= 0f) fDashCooldown = cfg.dashCooldown;

        if (cfg.airFriction >= 0f) fAirFriction = cfg.airFriction;
        if (cfg.groundFriction >= 0f) fGroundFriction = cfg.groundFriction;

        if (cfg.gravity != -9999f) fGravity = cfg.gravity;

        // Yuki zeroG non ci serve qui
        bIsOniInfiniteDashActive = false;
    }

    // -------------------------------------------------------
    // Wall interaction (Oni)
    // -------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Se usi trigger walls
        TryOniWallStun(other);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Se usi collider normali
        TryOniWallStun(hit.collider);
    }

    private void TryOniWallStun(Collider col)
    {
        if (activeMaskConfig == null) return;
        if (activeMaskConfig.maskType != MaskType.Oni) return;
        if (!bIsOniInfiniteDashActive) return;

        if (!string.IsNullOrEmpty(activeMaskConfig.wallTag) && col.CompareTag(activeMaskConfig.wallTag))
        {
            if (corOniStunCoroutine != null) StopCoroutine(corOniStunCoroutine);
            corOniStunCoroutine = StartCoroutine(OniWallStunRoutine());
        }
    }

    private IEnumerator OniWallStunRoutine()
    {
        bIsStunned = true;
        bIsOniInfiniteDashActive = false;

        // reset immediato + stop motion
        SetDefaultStats();
        velocity = Vector3.zero;

        float stun = Mathf.Max(0.01f, activeMaskConfig.oniWallStunSeconds);
        yield return new WaitForSeconds(stun);

        bIsStunned = false;
        corOniStunCoroutine = null;
    }

    // -------------------------------------------------------
    // Public setters (optional)
    // -------------------------------------------------------
    public void SetMaxJumps(int value) => iMaxJumps = Mathf.Max(1, value);
    public void SetJumpForce(float value) => fJumpForce = value;
    public void SetDashSpeed(float value) => fDashSpeed = value;

    // -------------------------------------------------------
    // Respawn
    // -------------------------------------------------------
    public void SetRespawnPoint(Vector3 newPos)
    {
        currentRespawnPoint = newPos;
    }

    public void TeleportTo(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;

        velocity = Vector3.zero;
    }

    // Settings UI
    public void SetGameplayControl(bool isActive)
    {
        // 1. Imposta la variabile che blocca/sblocca input e visuale
        bIsGameplay = isActive;

        // 2. Forza l'aggiornamento immediato del cursore usando la TUA logica esistente
        ApplyCursorState(); 
    }
}
