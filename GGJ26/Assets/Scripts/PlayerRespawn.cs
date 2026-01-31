using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 startPosition;

    void Start()
    {
        // Memorizza la posizione (x, y, z) iniziale
        startPosition = transform.position;
    }

    public void Respawn()
    {
        // Reset of position
        transform.position = startPosition;

        // Reset velocity
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; 
        }
    }
}