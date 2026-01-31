using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private PlayerCharacterController player;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check player controller is possessed
        if (other.CompareTag("Player"))
        {
            // Update the position of respawn
            player.SetRespawnPoint(transform.position);
        }
    }
}