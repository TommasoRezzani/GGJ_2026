using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    // Trascina qui il pannello del Pop-up creato nella UI
    [SerializeField] private GameObject quitConfirmationPanel;

    private void Start()
    {
        // Assicuriamoci che il pop-up sia chiuso all'avvio
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    // --- NUOVE FUNZIONI PER IL POP-UP ---

    // 1. Collegare questo al tasto "Quit" del Menu Principale
    public void ShowQuitConfirmation()
    {
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(true); // Mostra il pop-up
        }
    }

    // 2. Collegare questo al tasto "Yes" del Pop-up
    public void ConfirmQuit()
    {
        Application.Quit();
        Debug.Log("Player has quit the game.");
    }

    // 3. Collegare questo al tasto "No" del Pop-up
    public void CancelQuit()
    {
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false); // Nasconde il pop-up
        }
    }
}