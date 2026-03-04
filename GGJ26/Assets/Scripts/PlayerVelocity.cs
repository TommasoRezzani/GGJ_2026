using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerVelocity : MonoBehaviour
{
    private CharacterController cc;

    [SerializeField] private float currentSpeed;

    void Awake() => cc = GetComponent<CharacterController>();

    void Update()
    {
        // Velocità reale calcolata dal CC
        Vector3 v = cc.velocity;
        v.y = 0f;
        currentSpeed = v.magnitude;
    }
}