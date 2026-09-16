using UnityEngine;

// Fits the orthographic camera so the tank always fills the screen width.
// On shorter screens the top of the water is cropped; the sand stays visible.
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [SerializeField] Vector2 worldSize = new Vector2(1.35f, 3f);
    [SerializeField] Vector2 worldCenter = Vector2.zero;

    Camera cam;

    public static CameraFit Main { get; private set; }

    // World-space rectangle currently visible on screen.
    public Rect VisibleRect
    {
        get
        {
            if (cam == null) return new Rect(worldCenter - worldSize * 0.5f, worldSize);
            float h = cam.orthographicSize * 2f;
            float w = h * cam.aspect;
            Vector2 c = cam.transform.position;
            return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
        }
    }

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        Main = this;
        Fit();
    }

    void LateUpdate() => Fit();

    void Fit()
    {
        if (cam == null || cam.aspect <= 0f) return;
        // Fill width; if the screen is even narrower than the art, fill height instead.
        float sizeForWidth = worldSize.x * 0.5f / cam.aspect;
        float sizeForHeight = worldSize.y * 0.5f;
        cam.orthographicSize = Mathf.Min(sizeForWidth, sizeForHeight);

        // Anchor the view to the bottom of the tank.
        float bottom = worldCenter.y - worldSize.y * 0.5f;
        Vector3 p = cam.transform.position;
        cam.transform.position = new Vector3(worldCenter.x, bottom + cam.orthographicSize, p.z);
    }
}
