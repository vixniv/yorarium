using UnityEngine;

// Swims to random points inside the swim area, facing the travel direction.
// Another component can take over steering with SetTarget (e.g. chasing food).
public class FishWander : MonoBehaviour
{
    [SerializeField] SwimArea area;
    [SerializeField] SpriteRenderer body;
    [SerializeField] float speed = 0.25f;
    [SerializeField] Vector2 pauseRange = new Vector2(0.3f, 1.5f);
    [SerializeField] Vector2 margin = new Vector2(0.1f, 0.07f);
    // True when the sprite art faces left.
    [SerializeField] bool artFacesLeft = true;

    Vector2 target;
    float pauseTimer;
    Vector2 velocity;

    bool hasOverride;
    Vector2 overrideTarget;
    float overrideSpeedMultiplier = 1f;

    public SpriteRenderer Body => body;

    void Start()
    {
        if (area == null) area = SwimArea.Main;
        PickTarget();
    }

    public void SetTarget(Vector2 point, float speedMultiplier)
    {
        hasOverride = true;
        overrideTarget = point;
        overrideSpeedMultiplier = speedMultiplier;
    }

    public void ClearTarget()
    {
        if (!hasOverride) return;
        hasOverride = false;
        pauseTimer = 0f;
        PickTarget();
    }

    void Update()
    {
        if (area == null) return;

        if (hasOverride)
        {
            Vector2 toTarget = overrideTarget - (Vector2)transform.position;
            // Only ease off in the last few pixels, so fish can still catch sinking food.
            Vector2 desired = Vector2.ClampMagnitude(toTarget * 25f, 1f) * speed * overrideSpeedMultiplier;
            velocity = Vector2.Lerp(velocity, desired, Time.deltaTime * 5f);
        }
        else if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 4f);
        }
        else
        {
            Vector2 toTarget = target - (Vector2)transform.position;
            if (toTarget.magnitude < 0.02f)
            {
                pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
                PickTarget();
            }
            else
            {
                Vector2 desired = toTarget.normalized * speed;
                velocity = Vector2.Lerp(velocity, desired, Time.deltaTime * 3f);
            }
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (body != null && Mathf.Abs(velocity.x) > 0.01f)
            body.flipX = (velocity.x > 0f) == artFacesLeft;
    }

    void PickTarget() => target = area != null ? area.RandomPoint(margin) : (Vector2)transform.position;
}
