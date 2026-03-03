using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerCharacterController moveController;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private Rigidbody rb;

    void Start()
    {
        moveController = GetComponent<PlayerCharacterController>();
        rb = GetComponent<Rigidbody>();
        
        // Last position setted for lastCheckpoint
        lastCheckpointPosition = transform.position;
        moveController = GetComponent<PlayerCharacterController>();
        lastCheckpointPosition = transform.position;
        lastCheckpointRotation = transform.rotation;
    }

    // -------------------
    // COLLISION OF PLAYER
    // -------------------
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Kill"))
        {
            Respawn();
        }
    }

    // --------------------------------------------
    // CASO B: TRIGGER OF PLAYER (Checkpoint, Fire)
    // --------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Checkpoint
        if (other.CompareTag("Respawn"))
        {
            lastCheckpointPosition = other.transform.position;
            lastCheckpointRotation = other.transform.rotation;
            Debug.Log("Checkpoint Salvato!");
        }

        //Trap
        if (other.CompareTag("Kill"))
        {
            Respawn();
        }
    }

    //Respawn
    private void Respawn()
    {
        if (moveController != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            moveController.TeleportTo(lastCheckpointPosition, lastCheckpointRotation);
        }
    }
}