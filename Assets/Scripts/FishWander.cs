using UnityEngine;

// Swims to random points inside the swim area, facing the travel direction.
// Another component can take over steering with SetTarget (e.g. chasing food).
// DropIn makes a new fish fall into the tank from above before it starts swimming.
public class FishWander : MonoBehaviour
{
    [SerializeField] SwimArea area;
    [SerializeField] SpriteRenderer body;
    [SerializeField] float speed = 0.25f;
    [SerializeField] Vector2 pauseRange = new Vector2(0.3f, 1.5f);
    [SerializeField] Vector2 margin = new Vector2(0.1f, 0.07f);
    // True when the sprite art faces left.
    [SerializeField] bool artFacesLeft = true;
    [Header("Drop in")]
    [SerializeField] float dropGravity = 2f;
    [SerializeField] float dropMaxSpeed = 1.2f;
    // Side-to-side sway while falling, in world units.
    [SerializeField] float dropWobble = 0.03f;

    Vector2 target;
    float pauseTimer;
    Vector2 velocity;

    bool hasOverride;
    Vector2 overrideTarget;
    float overrideSpeedMultiplier = 1f;

    bool dropping;
    float dropLandY;
    float dropSpeed;
    float dropTime;
    float dropX;

    public SpriteRenderer Body => body;
    public bool IsDropping => dropping;

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

    // Falls straight down from the current position, then settles into normal swimming at landY.
    public void DropIn(float landY)
    {
        dropping = true;
        dropLandY = landY;
        dropSpeed = 0f;
        dropTime = 0f;
        dropX = transform.position.x;
        velocity = Vector2.zero;
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
        if (dropping)
        {
            Fall();
            return;
        }

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

    void Fall()
    {
        dropTime += Time.deltaTime;
        dropSpeed = Mathf.Min(dropSpeed + dropGravity * Time.deltaTime, dropMaxSpeed);
        Vector3 p = transform.position;
        p.y -= dropSpeed * Time.deltaTime;
        p.x = dropX + Mathf.Sin(dropTime * 9f) * dropWobble;
        if (p.y <= dropLandY)
        {
            p.y = dropLandY;
            dropping = false;
            // Carry a little of the fall into the first swim so it doesn't stop dead.
            velocity = Vector2.down * Mathf.Min(dropSpeed, speed);
            pauseTimer = 0f;
            PickTarget();
        }
        transform.position = p;
    }

    void PickTarget() => target = area != null ? area.RandomPoint(margin) : (Vector2)transform.position;
}
