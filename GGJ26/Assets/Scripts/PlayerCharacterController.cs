using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [Header("Look")][SerializeField] private float fMouseSensitivity = 0.12f;
    [SerializeField] private float fPitchMin = -80f;
    [SerializeField] private float fPitchMax = 80f;

    private float yaw;
    private float pitch;

    [Header("Boolean")][SerializeField] bool bIsGameplay = true;


    [Header("Movement")][SerializeField] private float fMoveSpeed = 6f;

    [Header("Jump")][SerializeField] private float fJumpForce = 7f;
    [SerializeField] private int fMaxJumps = 1;

    [Header("Dash")][SerializeField] private float fDashSpeed = 18f;
    [SerializeField] private float fDashCooldown = 0.5f;
    private bool bDashOnlyWhenGrounded = false;

    [Header("References")]
    [SerializeField]
    private Transform view; // Camera o pivot

    [Header("Gravity")][SerializeField] private float fGravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;

    private int iJumpsRemaining;
    private float fLastDashTime;

    private Vector3 currentRespawnPoint;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        iJumpsRemaining = fMaxJumps;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        if (view != null)
        {
            pitch = view.eulerAngles.x;
        }

        ApplyCursorState();
    }

    void Update()
    {
        HandleMovement_NewInputSystem();
        HandleJump_NewInputSystem();
        HandleDash_NewInputSystem();
        ApplyGravity();
        HandleLook();

        controller.Move(velocity * Time.deltaTime);
    }


    // ----------------------
    // MOVEMENT (corsa)
    // ----------------------
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

        Vector3 moveDir;
        if (view != null)
            moveDir = (view.forward * input.y + view.right * input.x);
        else
            moveDir = (transform.forward * input.y + transform.right * input.x);

        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        velocity.x = moveDir.x * fMoveSpeed;
        velocity.z = moveDir.z * fMoveSpeed;
    }

    // ----------------------
    // JUMP
    // ----------------------
    private void HandleJump_NewInputSystem()
    {
        if (controller.isGrounded)
        {
            iJumpsRemaining = fMaxJumps;

            if (velocity.y < 0f)
                velocity.y = -2f;
        }

        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed && iJumpsRemaining > 0)
        {
            velocity.y = fJumpForce;
            iJumpsRemaining--;
        }
    }

    // ----------------------
    // DASH (override forward)
    // ----------------------
    private void HandleDash_NewInputSystem()
    {
        if (Time.time < fLastDashTime + fDashCooldown)
            return;

        if (bDashOnlyWhenGrounded && !controller.isGrounded)
            return;

        bool dashPressed = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;

        if (dashPressed)
        {
            Vector3 dashDir = view != null ? view.forward : transform.forward;
            dashDir.y = 0f;
            dashDir.Normalize();

            // OVERRIDE: niente somma vettoriale
            velocity.x = dashDir.x * fDashSpeed;
            velocity.z = dashDir.z * fDashSpeed;

            fLastDashTime = Time.time;
        }
    }

    // ----------------------
    // GRAVITY
    // ----------------------
    private void ApplyGravity()
    {
        velocity.y += fGravity * Time.deltaTime;
    }

    //Update cursor view
    private void HandleLook()
    {
        if (!bIsGameplay) return;
        if (view == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        //Delta Mouse (pixel/frame)
        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * fMouseSensitivity;
        pitch -= delta.y * fMouseSensitivity;

        pitch = Mathf.Clamp(pitch, fPitchMin, fPitchMax);

        // Yaw on Player (rotate on body)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        //Pitch on pivot/camera (only X)
        view.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    // Cursor View is locked
    private void ApplyCursorState()
    {
        Cursor.lockState = bIsGameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bIsGameplay;
    }

    // ----------------------
    // API PUBBLICHE
    // ----------------------
    public void SetMaxJumps(int value) => fMaxJumps = Mathf.Max(1, value);
    public void SetJumpForce(float value) => fJumpForce = value;
    public void SetDashSpeed(float value) => fDashSpeed = value;

    // Checkpoint based on triggers
    public void SetRespawnPoint(Vector3 newPos)
    {
        currentRespawnPoint = newPos;
    }

    // Death managed with teleport
    public void TeleportTo(Vector3 position)
    {
        //Disable the controller to limit the conflict
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;

        // Reset velocity
        velocity = Vector3.zero;
    }
}