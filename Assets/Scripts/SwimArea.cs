using UnityEngine;

// World-space rectangle creatures are allowed to swim in.
public class SwimArea : MonoBehaviour
{
    [SerializeField] Rect bounds = new Rect(-0.58f, -0.85f, 1.16f, 1.9f);
    // Height where sinking food and coins come to rest on the sand.
    [SerializeField] float sandY = -1f;
    // Space kept free under the top HUD on short screens.
    [SerializeField] float hudTopMargin = 0.3f;

    public static SwimArea Main { get; private set; }

    public float SandY => sandY;

    // Swim bounds, with the top clamped to what is visible below the HUD.
    public Rect Bounds
    {
        get
        {
            Rect r = bounds;
            if (CameraFit.Main != null)
            {
                float visibleTop = CameraFit.Main.VisibleRect.yMax - hudTopMargin;
                if (visibleTop < r.yMax) r.yMax = Mathf.Max(visibleTop, r.yMin + 0.2f);
            }
            return r;
        }
    }

    void Awake() => Main = this;

    public Vector2 RandomPoint(Vector2 margin)
    {
        Rect r = Bounds;
        return new Vector2(
            Random.Range(r.xMin + margin.x, r.xMax - margin.x),
            Random.Range(r.yMin + margin.y, r.yMax - margin.y));
    }

    public Vector2 Clamp(Vector2 p)
    {
        Rect r = Bounds;
        return new Vector2(Mathf.Clamp(p.x, r.xMin, r.xMax), Mathf.Clamp(p.y, r.yMin, r.yMax));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(bounds.xMin, sandY), new Vector3(bounds.xMax, sandY));
    }
}
