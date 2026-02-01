using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    // Variabile statica per accedere a questo oggetto da ovunque
    public static BackgroundMusicController Instance;

    private void Awake()
    {
        // Logica Singleton: se esiste già una musica (es. torni dalle impostazioni),
        // distruggi quella nuova per non averne due sovrapposte.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // COMANDO MAGICO: Sopravvive al cambio scena!
    }

    // Funzione da chiamare quando inizia il gioco per uccidere la musica
    public void StopMusic()
    {
        Destroy(gameObject);
    }
}