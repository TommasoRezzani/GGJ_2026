using UnityEngine;
using UnityEngine.InputSystem; 

public class PauseController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Il pannello delle Impostazioni (Slider, ecc.)")]
    [SerializeField] private GameObject settingsPanel;

    [Tooltip("Il pannello piccolo 'Vuoi Uscire?'")]
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("References")]
    [SerializeField] private PlayerCharacterController playerController;

    // Stato interno
    private bool isPaused = false;

    void Start()
    {
        // All'avvio nascondiamo tutto e sblocchiamo il tempo
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(false);
        
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // --- TASTO P: Apre/Chiude SETTINGS ---
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            // Se il pannello Quit è aperto, lo chiudiamo prima di aprire Settings
            if (quitConfirmationPanel.activeSelf) 
            {
                CloseAllPanels();
                TogglePause(settingsPanel);
            }
            else
            {
                TogglePause(settingsPanel);
            }
        }

        // --- TASTO ESC: Apre/Chiude QUIT POPUP ---
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Se il pannello Settings è aperto, lo chiudiamo prima di aprire Quit
            if (settingsPanel.activeSelf)
            {
                CloseAllPanels();
                TogglePause(quitConfirmationPanel);
            }
            else
            {
                TogglePause(quitConfirmationPanel);
            }
        }
    }

    // Logica centrale per gestire la pausa e mostrare UN pannello specifico
    private void TogglePause(GameObject panelToOpen)
    {
        // Se il pannello richiesto è già aperto, stiamo "riprendendo" il gioco
        bool isClosing = panelToOpen.activeSelf;

        if (isClosing)
        {
            ResumeGame();
        }
        else
        {
            PauseGame(panelToOpen);
        }
    }

    private void PauseGame(GameObject panelToShow)
    {
        isPaused = true;
        Time.timeScale = 0.0001f; // Ferma il tempo

        // Attiva solo il pannello richiesto
        if (panelToShow != null) panelToShow.SetActive(true);

        // Sblocca il cursore
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Blocca i movimenti del player
        if (playerController != null) playerController.SetGameplayControl(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Riparte il tempo

        // Chiude TUTTI i pannelli per sicurezza
        CloseAllPanels();

        // Blocca il cursore e riattiva il player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) playerController.SetGameplayControl(true);
    }

    private void CloseAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(false);
    }

    // --- FUNZIONI PER I BOTTONI UI (Da collegare nell'Inspector) ---
    
    // Collega al tasto "NO" o "Resume"
    public void ButtonResume() 
    {
        ResumeGame();
    }

    // Collega al tasto "YES" o "Quit Game"
    public void ButtonQuitGame()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit();
    }
}