using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per il New Input System

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Trascina qui il GameObject 'SettingsCanvas' o il pannello nero di sfondo")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Player Reference")]
    [Tooltip("Trascina qui il Player dalla scena")]
    [SerializeField] private PlayerCharacterController playerController;

    // Stato interno
    private bool isPaused = false;

    void Start()
    {
        // All'avvio, assicuriamoci che il menu sia nascosto e il tempo scorra
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Controlla se la tastiera è collegata prima di leggere l'input
        if (Keyboard.current == null) return;

        // Se premi ESC o P -> Attiva/Disattiva pausa
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // Funzione pubblica: utile se vuoi richiamare la pausa anche da un tasto UI
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
        // 1. Ferma il tempo
        Time.timeScale = 0.0001f;

        // 2. Mostra il Menu Opzioni
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // 3. Blocca il Player e SBLOCCA il Cursore
        if (playerController != null)
        {
            // False = Non siamo in gameplay -> Cursore visibile, Player fermo
            playerController.SetGameplayControl(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false; // Sicurezza per allineare lo stato

        // 1. Ripristina il tempo normale
        Time.timeScale = 1f;

        // 2. Nascondi il Menu Opzioni
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 3. Sblocca il Player e BLOCCA il Cursore
        if (playerController != null)
        {
            // True = Siamo in gameplay -> Cursore nascosto, Player si muove
            playerController.SetGameplayControl(true);
        }
    }
    
    // COLLEGA QUESTA FUNZIONE AL TASTO "BACK" NELL'INSPECTOR DI UNITY
    public void CloseMenuButton()
    {
        // Funziona solo se il gioco è effettivamente in pausa
        if (isPaused) 
        {
            TogglePause();
        }
    }
}