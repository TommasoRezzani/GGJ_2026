using UnityEngine;

public class FlameWallController : MonoBehaviour
{
    [SerializeField] private float minSpeedToPass = 15f;

    [SerializeField] private Collider solidCollider;

    [SerializeField] private Renderer flameRenderer;

    private PlayerCharacterController PlayerCharacterController;

    // [SerializeField] private ParticleSystem futureFlameParticles; // Da collegare in futuro

    private void Start()
    {
        SetWallActive(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerScript = other.GetComponent<PlayerCharacterController>(); 

            if (playerScript != null)
            {
                // Confrontiamo la tua variabile fMoveSpeed con il limite del muro
                if (PlayerCharacterController.fMoveSpeed >= minSpeedToPass)
                {
                    SetWallActive(false); // PASSAGGIO APERTO
                }
                else
                {
                    SetWallActive(true);  // PASSAGGIO CHIUSO
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetWallActive(true);
        }
    }

    private void SetWallActive(bool isActive)
    {
        // 1. Gestione Collider Fisico
        if (solidCollider != null) solidCollider.enabled = isActive;

        // 2. Gestione Visiva (Texture)
        // Se il muro è attivo (isActive = true), mostriamo la texture. 
        // Se è disattivo (passi), la nascondiamo.
        if (flameRenderer != null) flameRenderer.enabled = isActive;
        
    }
}