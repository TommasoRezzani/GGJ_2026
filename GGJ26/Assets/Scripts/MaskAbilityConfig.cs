using UnityEngine;

/// <summary>
/// Identifies which gameplay logic branch this mask uses.
/// </summary>
public enum MaskType
{
    Default,
    Yuki,
    Oni
}

[CreateAssetMenu(menuName = "Game/Mask Ability Config")]
public class MaskAbilityConfig : ScriptableObject
{
    // ------------------------------------------------------------------
    // IDENTITY
    // ------------------------------------------------------------------

    [Header("Identity")]
    [Tooltip("Defines which special gameplay logic this mask uses (Default / Yuki / Oni).")]
    public MaskType maskType = MaskType.Default;

    // ------------------------------------------------------------------
    // COMMON OVERRIDES
    // ------------------------------------------------------------------

    [Header("Common Overrides (-1 = keep player's default values)")]

    [Tooltip(
        "Overrides the player's movement speed.\n" +
        "Use -1 to keep the player's default speed.\n\n" +
        "Higher value = faster horizontal movement."
    )]
    public float moveSpeed = -1f;

    [Tooltip(
        "Overrides the player's jump force.\n" +
        "Use -1 to keep the player's default jump force.\n\n" +
        "Higher value = higher jumps."
    )]
    public float jumpForce = -1f;

    [Tooltip(
        "Overrides the maximum number of consecutive jumps before touching the ground.\n" +
        "Use -1 to keep the default value.\n\n" +
        "1 = single jump\n" +
        "2 = double jump\n" +
        "3 = triple jump"
    )]
    public int maxJumps = -1;

    // ------------------------------------------------------------------
    // FRICTION & GRAVITY
    // ------------------------------------------------------------------

    [Header("Friction & Gravity (-9999 / -1 = keep default)")]

    [Tooltip(
        "Overrides gravity applied to the player.\n" +
        "Use -9999 to keep the default gravity.\n\n" +
        "0 = no gravity (floating)\n" +
        "More negative values = stronger gravity."
    )]
    public float gravity = -9999f;

    [Tooltip(
        "Overrides air friction / air control.\n" +
        "Use -1 to keep default.\n\n" +
        "Higher value = more air control (less floaty).\n" +
        "Lower value = more inertia / floaty movement."
    )]
    public float airFriction = -1f;

    [Tooltip(
        "Overrides ground friction / ground control.\n" +
        "Use -1 to keep default.\n\n" +
        "Higher value = more control, stops faster.\n" +
        "Lower value = more sliding / inertia."
    )]
    public float groundFriction = -1f;

    // ------------------------------------------------------------------
    // DASH
    // ------------------------------------------------------------------

    [Header("Dash (-1 = keep default)")]

    [Tooltip(
        "Overrides dash speed.\n" +
        "Use -1 to keep default.\n\n" +
        "Higher value = faster dash."
    )]
    public float dashSpeed = -1f;

    [Tooltip(
        "Overrides dash cooldown.\n" +
        "Use -1 to keep default.\n\n" +
        "Lower value = dash can be used more frequently."
    )]
    public float dashCooldown = -1f;

    // ------------------------------------------------------------------
    // YUKI ABILITY
    // ------------------------------------------------------------------

    [Header("Yuki Ability")]

    [Tooltip(
        "If enabled, the player will temporarily have zero gravity.\n" +
        "Used to simulate a short 'fly' or floating state."
    )]
    public bool yukiEnableZeroGravity = false;

    [Tooltip(
        "Duration (in seconds) of Yuki's zero gravity effect.\n" +
        "Only used if Zero Gravity is enabled."
    )]
    public float yukiZeroGravityDuration = 2.0f;

    // ------------------------------------------------------------------
    // ONI ABILITY
    // ------------------------------------------------------------------

    [Header("Oni Ability")]

    [Tooltip(
        "If enabled, the player enters Infinite Dash mode while holding the Dash input.\n" +
        "Infinite Dash is interrupted by:\n" +
        "- Jumping\n" +
        "- Hitting a Wall\n"
    )]
    public bool oniInfiniteDash = false;

    [Tooltip(
        "If the player jumps during Oni Infinite Dash,\n" +
        "this delay is waited before resetting stats to default.\n\n" +
        "Use 0 for instant reset."
    )]
    public float oniResetDelayAfterSpace = 1.0f;

    [Tooltip(
        "After Oni reset, stats are gradually restored over this time\n" +
        "while the player is moving.\n\n" +
        "Higher value = slower, smoother recovery."
    )]
    public float oniRecoveryDuration = 2.0f;

    [Tooltip(
        "If the player hits a Wall during Oni Infinite Dash,\n" +
        "the player is stunned for this duration (seconds)\n" +
        "and stats are reset to default."
    )]
    public float oniWallStunSeconds = 1.0f;

    [Tooltip(
        "Tag used to identify walls that stop Oni Infinite Dash.\n" +
        "The wall collider must have this tag."
    )]
    public string wallTag = "Wall";

    // ------------------------------------------------------------------
    // ONI JUMP SCALING
    // ------------------------------------------------------------------

    [Header("Oni Jump Scaling")]

    [Tooltip(
        "If enabled, Oni's jump strength increases with horizontal speed.\n" +
        "Higher speed = stronger jump."
    )]
    public bool bOniJumpScaleWithSpeed = false;

    [Tooltip(
        "Minimum jump multiplier when the player is barely moving.\n" +
        "1 = no bonus.\n" +
        "Values below 1 reduce jump strength."
    )]
    public float fOniJumpMinMultiplier = 1f;

    [Tooltip(
        "Maximum jump multiplier when reaching max Oni speed.\n" +
        "Higher value = stronger speed-based jumps."
    )]
    public float fOniJumpMaxMultiplier = 1.6f;

    [Tooltip(
        "Horizontal speed required to reach the maximum jump multiplier.\n" +
        "Lower value = jump boosts earlier.\n" +
        "Higher value = requires more speed to fully boost."
    )]
    public float fOniSpeedForMaxJump = 25f;
}
