using UnityEngine;

public class FlameWallController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Velocità minima per passare (es. 15 se Oni corre a 20)")]
    [SerializeField] private float minSpeedToPass = 15f; 

    [Header("Audio")]
    [SerializeField] private AudioSource wallAudioSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    
    // Riferimenti interni (li troviamo in automatico)
    private Collider myCollider;
    private Renderer myRenderer;

    // Cooldown per non spammare il suono di errore
    private float lastFailTime = 0f;
    private float failCooldown = 1.0f;

    private void Start()
    {
        // AUTO-SETUP: Ci prendiamo i componenti da soli
        myCollider = GetComponent<Collider>();
        myRenderer = GetComponent<Renderer>();

        // Sicurezza: se manca l'AudioSource ma lo script è su un oggetto che ne ha uno
        if (wallAudioSource == null)
            wallAudioSource = GetComponent<AudioSource>();

        SetWallActive(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerScript = other.GetComponent<PlayerCharacterController>(); 

            if (playerScript != null)
            {
                // Leggiamo la velocità dal Player (grazie alle modifiche fatte prima)
                float currentSpeed = playerScript.CurrentSpeed; 

                if (currentSpeed >= minSpeedToPass)
                {
                    // --- SUCCESSO ---
                    // Se il collider è ancora attivo, vuol dire che lo stiamo aprendo ora
                    if (myCollider != null && myCollider.enabled)
                    {
                        PlaySound(successClip);
                        SetWallActive(false); // Nascondi e Disattiva
                    }
                }
                else
                {
                    // --- FALLIMENTO ---
                    // Il PlayerCharacterController gestirà il rimbalzo fisico.
                    // Noi gestiamo solo il suono "BONK".
                    
                    if (Time.time > lastFailTime + failCooldown)
                    {
                        PlaySound(failClip);
                        lastFailTime = Time.time;
                    }
                }
            }
        }
    }

    // Funzione per riattivare il muro quando il giocatore esce dalla zona (per reset o respawn)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetWallActive(true);
        }
    }

    private void SetWallActive(bool isActive)
    {
        // Disattiva il trigger (così non suona più) e la grafica
        if (myCollider != null) myCollider.enabled = isActive;
        if (myRenderer != null) myRenderer.enabled = isActive;
    }

    private void PlaySound(AudioClip clip)
    {
        if (wallAudioSource != null && clip != null)
        {
            wallAudioSource.PlayOneShot(clip);
        }
    }
}