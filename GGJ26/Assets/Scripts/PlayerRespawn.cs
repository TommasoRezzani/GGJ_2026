using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 startPosition;
    private Rigidbody rb;
    
    void Start()
    {
        // Memorizza la posizione (x, y, z) iniziale
        startPosition = transform.position;
    }

    public void Respawn()
    {        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}