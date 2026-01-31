using UnityEngine;

public class SpikeController : PlatformController
{
    private void OnTriggerEnter(Collider other)
    {
        // 'other' è il Collider che è entrato nel trigger
        // Cerchiamo se l'oggetto ha lo script PlayerRespawn3D
        if (other.TryGetComponent<PlayerRespawn>(out PlayerRespawn player))
        {
            player.Respawn();
        }
    }
}
