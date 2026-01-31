using UnityEngine;

public class MovingPlatformController : PlatformController
{
    [Header("Moving Platform Settings")]
    [Header("Speed")] [SerializeField] private float fPlatSpeed = 2.0f;
    public int iPlatStartPoint;
    public Transform[] points;
    private int i;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = points[iPlatStartPoint].position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, points[i].position) < 0.02f)
        {
            i++;
            if (i == points.Length)
            {
                i = 0;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, points[i].position,
                  fPlatSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(transform);
    }
    
    private void OnCollisionExit(Collision collision)
    {
        collision.transform.SetParent(null);
    }
}
