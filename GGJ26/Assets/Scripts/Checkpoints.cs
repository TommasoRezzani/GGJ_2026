using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Cerca il player usando il nuovo sistema
        if (other.TryGetComponent<PlayerCharacterController>(out PlayerCharacterController player))
        {
            // Aggiorna il punto di respawn del player con la posizione di QUESTO checkpoint
            player.SetRespawnPoint(transform.position);
        }
    }
}