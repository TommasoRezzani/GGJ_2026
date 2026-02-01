using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    // Variabile di stato: una volta diventata true, non tornerà mai più false
    private bool hasPlayed = false; 

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Controlliamo se è il player E se il suono non è già stato suonato
        if (other.CompareTag("Player"))
        {
            PlayerCharacterController playerScript = other.GetComponent<PlayerCharacterController>();

            if (playerScript != null)
            {
                // 1. Salviamo SEMPRE la posizione (utile se vuoi aggiornare lo spawn point ripassandoci)
                playerScript.SetRespawnPoint(transform.position);
                
                // 2. Gestione Suono "One Time Only"
                if (!hasPlayed)
                {
                    PlayActivationSound();
                    Debug.Log("Checkpoint Attivato per la prima volta!");
                }
            }
        }
    }

    // Ho RIMOSSO OnTriggerExit. 
    // Ora 'hasPlayed' rimane true per sempre finché non ricarichi la scena.

    private void PlayActivationSound()
    {
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
            hasPlayed = true; // Blocchiamo il suono per il futuro
        }
    }
}