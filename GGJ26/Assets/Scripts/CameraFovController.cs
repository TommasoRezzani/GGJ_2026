using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFovController : MonoBehaviour
{
    [SerializeField] private float defaultFov = 70f;
    [SerializeField] private float lerpSpeed = 8f;

    private Camera cam;
    private float targetFov;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (defaultFov <= 0f) defaultFov = cam.fieldOfView;
        targetFov = cam.fieldOfView;
    }

    void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));
    }

    public void SetFov(float fov, float? customLerpSpeed = null)
    {
        if (customLerpSpeed.HasValue) lerpSpeed = customLerpSpeed.Value;

        if (fov <= 0f) // fallback
            targetFov = defaultFov;
        else
            targetFov = fov;
    }

    public void ResetFov() => targetFov = defaultFov;
}