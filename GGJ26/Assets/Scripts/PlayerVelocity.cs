using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerVelocity : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] 
    private float currentSpeed;   // Visibile in Inspector ma non modificabile esternamente

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude;
    }
}