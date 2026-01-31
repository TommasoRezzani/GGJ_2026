using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerCharacterController moveController;
    private Vector3 lastCheckpointPosition;

    void Start()
    {
        moveController = GetComponent<PlayerCharacterController>();
        
        // Imposta la posizione iniziale come primo checkpoint di default
        lastCheckpointPosition = transform.position;
    }

    // ------------------------------------------------
    // CASO A: Collisione SOLIDA (Muri, Spine solide, Pavimento)
    // ------------------------------------------------
    // Questa funzione viene chiamata da Unity ogni volta che il CharacterController
    // tocca un altro collider mentre si muove.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Se tocco un oggetto solido taggato "Kill"
        if (hit.gameObject.CompareTag("Kill"))
        {
            Respawn();
        }
    }

    // ------------------------------------------------
    // CASO B: Collisione TRIGGER (Checkpoint, Lava, Fuoco, Zone d'aria)
    // ------------------------------------------------
    // Questa funzione serve per oggetti in cui PUOI entrare
    private void OnTriggerEnter(Collider other)
    {
        // Se entro in un checkpoint
        if (other.CompareTag("Respawn"))
        {
            lastCheckpointPosition = other.transform.position;
            Debug.Log("Checkpoint Salvato!");
        }

        // Gestisce anche la morte se la trappola è un trigger (es. un ologramma o del gas)
        if (other.CompareTag("Kill"))
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        Debug.Log("Giocatore morto. Respawn in corso...");
        if (moveController != null)
        {
            moveController.TeleportTo(lastCheckpointPosition);
        }
    }
}