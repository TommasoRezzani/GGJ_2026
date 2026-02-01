using UnityEngine;

public enum MaskType { Default, Yuki, Oni }

[CreateAssetMenu(menuName = "Game/Mask Ability Config", fileName = "NewMaskAbilityConfig")]
public class MaskAbilityConfig : ScriptableObject
{
    [Header("Mask Identity")]
    public MaskType maskType = MaskType.Default;

    [Header("Common Overrides (-1 = keep default)")]
    public float moveSpeed = -1f;
    public float jumpForce = -1f;
    public int maxJumps = -1;
    public float gravity = -9999f;
    public float airFriction = -1f;
    public float groundFriction = -1f;
    public float dashSpeed = -1f;
    public float dashCooldown = -1f;

    [Header("YUKI - Second Jump")]
    public bool bYukiEnableSecondJump = true;
    public float fYukiSecondJumpForce = 12f;

    [Header("ONI - Sprint Charge")]
    public float fOniChargeDelay = 0.5f;        // Dopo quanto tempo di corsa inizia a caricare
    public float fOniChargeRate = 0.5f;         // Velocità di incremento (moltiplicatore al secondo)
    public float fOniMaxSpeedMultiplier = 2.5f; // Limite massimo di velocità
    public float fOniDissipateRate = 1.0f;      // Velocità di perdita carica al rilascio di Shift
    public float fOniSlideFactor = 2.0f;        // Quanto "slitta" il giocatore quando rilascia Shift

    [Header("ONI - Momentum Jump")]
    public bool bOniEnableMomentumJump = true;
    public float fOniMomentumJumpMultiplier = 1.2f; // Boost orizzontale extra al salto

    [Header("ONI - Wall Interaction")]
    public string wallTag = "Wall";
    [Range(0.8f, 1f)] public float fOniMaxSpeedThreshold01 = 0.95f; // Soglia per superare i muri (95% della velocità max)
}