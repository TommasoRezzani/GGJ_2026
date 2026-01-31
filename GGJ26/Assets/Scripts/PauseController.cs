using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Trascina qui il GameObject 'Canvas Settings' o il pannello che vuoi aprire")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Player Reference")]
    [Tooltip("Trascina qui il Player per bloccare i controlli")]
    [SerializeField] private PlayerCharacterController playerController;

    private bool isPaused = false;

    void Start()
{
    // Assicuriamoci che il menu sia chiuso all'inizio
    if (settingsPanel != null)
    {
        settingsPanel.SetActive(false);
    }
}

    void Update()
    {
        // Controlla input ESC o P
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        // 1. Ferma il tempo (fisica e animazioni)
        Time.timeScale = 0.0001f;

        // 2. Mostra la UI
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // 3. Disabilita controlli player e sblocca il mouse
        if (playerController != null)
        {
            playerController.SetGameplayControl(false);
        }
    }

    public void ResumeGame()
    {
        // 1. Ripristina il tempo
        Time.timeScale = 1f;

        // 2. Nascondi la UI
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 3. Riabilita controlli player e blocca il mouse
        if (playerController != null)
        {
            playerController.SetGameplayControl(true);
        }
    }
    
    // Funzione utile da collegare al tasto "Back" o "Close" dentro il menu Settings
    public void CloseMenuButton()
    {
        if(isPaused) TogglePause();
    }
}