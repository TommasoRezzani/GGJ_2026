using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMaskHandler : MonoBehaviour
{
    [Serializable]
    public class MaskEntry
    {
        [Tooltip("Visual object of the mask (Quad / UI overlay).")]
        public GameObject goMaskObject;

        [Tooltip("Gameplay configuration associated with this mask.")]
        public MaskAbilityConfig maskConfig;
    }

    // --------------------------------------------------
    // Inspector
    // --------------------------------------------------

    [Header("Masks Setup")]
    [SerializeField] private List<MaskEntry> lMasks = new List<MaskEntry>();

    [Tooltip("Initial mask index at game start.")]
    [SerializeField] private int iStartMaskIndex = 0;

    [Header("Input")]
    [Tooltip("Key used to cycle masks.")]
    [SerializeField] private Key kCycleMaskKey = Key.Q;

    [Header("Debug")]
    [SerializeField] private bool bLogMaskChanges = false;

    // --------------------------------------------------
    // Runtime
    // --------------------------------------------------

    private int iCurrentMaskIndex;
    private PlayerCharacterController playerController;

    // --------------------------------------------------
    // Unity Lifecycle
    // --------------------------------------------------

    private void Awake()
    {
        // Get player controller from parent hierarchy
        playerController = GetComponentInParent<PlayerCharacterController>();

        if (lMasks == null || lMasks.Count == 0)
        {
            iCurrentMaskIndex = 0;
            return;
        }

        // Clamp start index
        iCurrentMaskIndex = Mathf.Clamp(iStartMaskIndex, 0, lMasks.Count - 1);
    }

    private void Start()
    {
        // Activate initial mask
        if (lMasks != null && lMasks.Count > 0)
            ActivateMask(iCurrentMaskIndex);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[kCycleMaskKey].wasPressedThisFrame)
            CycleNextMask();
    }

    // --------------------------------------------------
    // Mask Logic
    // --------------------------------------------------

    /// <summary>
    /// Cycles to the next mask in the list.
    /// </summary>
    public void CycleNextMask()
    {
        if (lMasks == null || lMasks.Count == 0)
            return;

        iCurrentMaskIndex = (iCurrentMaskIndex + 1) % lMasks.Count;
        ActivateMask(iCurrentMaskIndex);
    }

    /// <summary>
    /// Cycles to the previous mask in the list.
    /// </summary>
    public void CyclePreviousMask()
    {
        if (lMasks == null || lMasks.Count == 0)
            return;

        iCurrentMaskIndex = (iCurrentMaskIndex - 1 + lMasks.Count) % lMasks.Count;
        ActivateMask(iCurrentMaskIndex);
    }

    /// <summary>
    /// Forces activation of a specific mask index.
    /// </summary>
    public void SetMaskByIndex(int iIndex)
    {
        if (lMasks == null || lMasks.Count == 0)
            return;

        iCurrentMaskIndex = Mathf.Clamp(iIndex, 0, lMasks.Count - 1);
        ActivateMask(iCurrentMaskIndex);
    }

    /// <summary>
    /// Activates the selected mask:
    /// - Enables its visual object
    /// - Applies its gameplay configuration to the player
    /// </summary>
    private void ActivateMask(int iIndex)
    {
        // ---------------------------
        // Visual activation
        // ---------------------------
        for (int i = 0; i < lMasks.Count; i++)
        {
            if (lMasks[i].goMaskObject != null)
                lMasks[i].goMaskObject.SetActive(i == iIndex);
        }

        // ---------------------------
        // Gameplay application
        // ---------------------------
        if (playerController != null)
        {
            MaskAbilityConfig config = lMasks[iIndex].maskConfig;

            if (config != null)
            {
                playerController.ApplyMask(config);

                if (bLogMaskChanges)
                    Debug.Log($"[MaskHandler] Applied mask: {config.name} ({config.maskType})");
            }
            else
            {
                // If no config is assigned, fallback to default player stats
                playerController.SetDefaultStats();

                if (bLogMaskChanges)
                    Debug.Log("[MaskHandler] No MaskAbilityConfig found. Reverted to default stats.");
            }
        }
        else
        {
            if (bLogMaskChanges)
                Debug.LogWarning("[MaskHandler] PlayerCharacterController not found in parent hierarchy.");
        }
    }

    // --------------------------------------------------
    // Getters (optional)
    // --------------------------------------------------

    public int GetCurrentMaskIndex() => iCurrentMaskIndex;

    public MaskAbilityConfig GetCurrentMaskConfig()
    {
        if (lMasks == null || lMasks.Count == 0)
            return null;

        return lMasks[iCurrentMaskIndex].maskConfig;
    }
}
