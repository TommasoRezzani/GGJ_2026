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
        // Riporta il player alla posizione iniziale
        transform.position = startPosition;

        // Resetta la velocità per evitare inerzia residua
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            // Nota: In Unity 6+ usa 'linearVelocity', nelle versioni precedenti usa 'velocity'
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; // Ferma anche la rotazione
        }
    }
}