using UnityEngine;

public class MovingPlatformController : MonoBehaviour // Ho rimesso MonoBehaviour per l'esempio, ma tu tieni pure PlatformController
{
    [Header("Speed")] 
    [SerializeField] private float fPlatSpeed = 2.0f;
    
    public int iPlatStartPoint;
    public Transform[] points;
    
    private int i;

    void Start()
    {
        // 1. Posizioniamo la piattaforma al punto di partenza
        transform.position = points[iPlatStartPoint].position;

        // 2. Impostiamo il prossimo target (il successivo nell'array)
        i = iPlatStartPoint + 1;
        if (i >= points.Length) i = 0;

        // --- SOLUZIONE AL TUO PROBLEMA ---
        // Stacchiamo i waypoint dalla piattaforma affinché restino fermi nel mondo
        // mentre la piattaforma si muove.
        foreach (Transform point in points)
        {
            point.parent = null; 
        }
    }

    // Usiamo FixedUpdate perché stai muovendo un oggetto fisico che trasporta il player
    void FixedUpdate()
    {
        // Controllo distanza
        if (Vector3.Distance(transform.position, points[i].position) < 0.02f)
        {
            i++;
            if (i == points.Length)
            {
                i = 0;
            }
        }

        // Movimento
        transform.position = Vector3.MoveTowards(transform.position, points[i].position, fPlatSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Nota: Assicurati che il Player non scali in modo strano quando diventa figlio
        collision.transform.SetParent(transform);
    }
    
    private void OnCollisionExit(Collision collision)
    {
        collision.transform.SetParent(null);
        // Opzionale: Se usi DontDestroyOnLoad per il player, assicurati che torni nella scena corretta
        // DontDestroyOnLoad(collision.gameObject); 
    }
}