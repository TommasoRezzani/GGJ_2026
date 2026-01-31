using UnityEngine;

public enum MaskType { Default, Yuki, Oni }

[CreateAssetMenu(menuName = "Game/Mask Ability Config")]
public class MaskAbilityConfig : ScriptableObject
{
    public MaskType maskType = MaskType.Default;

    [Header("Common Overrides (-1 = keep default)")]
    public float moveSpeed = -1f;
    public float jumpForce = -1f;
    public int maxJumps = -1;

    [Header("Friction & Gravity (-9999 = keep default)")]
    public float gravity = -9999f;
    public float airFriction = -1f;
    public float groundFriction = -1f;

    [Header("Dash (-1 = keep default)")]
    public float dashSpeed = -1f;
    public float dashCooldown = -1f;

    // ---------------- YUKI ----------------
    [Header("Yuki Ability")]
    public bool yukiEnableZeroGravity = false;
    public float yukiZeroGravityDuration = 2.0f;

    // ---------------- ONI ----------------
    [Header("Oni Ability")]
    public bool oniInfiniteDash = false;

    [Tooltip("Quando premi SPACE durante Oni infinite dash: dopo questo delay resetta a default")]
    public float oniResetDelayAfterSpace = 1.0f;

    [Tooltip("Dopo il reset: reincremento graduale delle stats in questo tempo (mentre ti muovi)")]
    public float oniRecoveryDuration = 2.0f;

    [Tooltip("Se in Oni dash tocchi un Wall: freeze per questi secondi + reset default")]
    public float oniWallStunSeconds = 1.0f;

    [Tooltip("Tag del trigger/muro che ferma l'Oni dash")]
    public string wallTag = "Wall";
    
    [Header("Oni Jump Scaling")]
    public bool bOniJumpScaleWithSpeed = false;
    public float fOniJumpMinMultiplier = 1f;
    public float fOniJumpMaxMultiplier = 1.6f;
    public float fOniSpeedForMaxJump = 25f;

}