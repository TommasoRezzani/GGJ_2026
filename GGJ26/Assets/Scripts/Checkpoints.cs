using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    // Variabile per evitare che il suono si ripeta all'infinito se ci stai dentro
    private bool hasPlayed = false; 

    private void Start()
    {
        // Se ti dimentichi di trascinare l'AudioSource, prova a prenderlo da solo
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // FIX: Dobbiamo prendere il componente DAL player che ci ha toccato
            PlayerCharacterController playerScript = other.GetComponent<PlayerCharacterController>();

            if (playerScript != null)
            {
                // 1. Salviamo la posizione
                playerScript.SetRespawnPoint(transform.position);
                Debug.Log("Checkpoint Attivato!");

                // 2. Suoniamo il feedback audio (solo se non sta già suonando)
                PlayActivationSound();
            }
        }
    }

    // Opzionale: Se vuoi che il checkpoint suoni ogni volta che ci ripassi, 
    // togli il controllo "!hasPlayed". 
    // Se vuoi che suoni una volta sola per partita, lascia così.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasPlayed = false; // Resettiamo così se torna indietro suona di nuovo
        }
    }

    private void PlayActivationSound()
    {
        if (audioSource != null && activationSound != null && !hasPlayed)
        {
            audioSource.PlayOneShot(activationSound);
            hasPlayed = true;
        }
    }
}