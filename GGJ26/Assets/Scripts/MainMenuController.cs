using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels References")]
    [Tooltip("Il pannello nero delle Impostazioni (SettingsCanvas)")]
    [SerializeField] private GameObject settingsPanel;
    
    [Tooltip("Il pannello di conferma uscita (QuitConfirmation)")]
    [SerializeField] private GameObject quitConfirmationPanel;

    [Tooltip("Il contenitore dei bottoni principali (Play, Settings, Quit) se vuoi nasconderli")]
    [SerializeField] private GameObject mainButtonsContainer; 

    private void Start()
    {
        // 1. All'avvio, NASCONDIAMO forzatamente i pannelli extra
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(false);
        
        // 2. Mostriamo i bottoni principali
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
    }

    public void StartGame()
    {
        // Ferma la musica (se usi il sistema singleton)
        if (BackgroundMusicController.Instance != null)
        {
            BackgroundMusicController.Instance.StopMusic();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // --- GESTIONE SETTINGS (PANNELLO) ---
    public void OpenSettings()
    {
        // Invece di caricare una scena, attiviamo il pannello che hai già lì
        if (settingsPanel != null) settingsPanel.SetActive(true);
        
        // Opzionale: nascondi i tasti del menu sotto per pulizia
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(false);
    }

    public void CloseSettings() // Collega questo al tasto "Back" dentro Settings
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
    }

    // --- GESTIONE QUIT POP-UP ---
    public void ShowQuitConfirmation()
    {
        if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(true);
    }

    public void ConfirmQuit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void CancelQuit()
    {
        if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(false);
    }
}