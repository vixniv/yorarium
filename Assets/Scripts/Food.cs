using UnityEngine;

// A food pellet that sinks to the sand, waits a bit, then fades away.
public class Food : MonoBehaviour
{
    [SerializeField] SpriteRenderer body;
    [SerializeField] float sinkSpeed = 0.15f;
    [SerializeField] float swayAmplitude = 0.015f;
    [SerializeField] float swayFrequency = 0.8f;
    [SerializeField] float restSeconds = 5f;
    [SerializeField] float fadeSeconds = 1f;

    public Fish ClaimedBy { get; set; }

    float baseX;
    float phase;
    float restTimer;
    bool resting;

    void Start()
    {
        baseX = transform.position.x;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float sandY = SwimArea.Main != null ? SwimArea.Main.SandY : -1f;
        Vector3 p = transform.position;

        if (!resting)
        {
            p.y -= sinkSpeed * Time.deltaTime;
            p.x = baseX + Mathf.Sin(Time.time * swayFrequency * Mathf.PI * 2f + phase) * swayAmplitude;
            if (p.y <= sandY)
            {
                p.y = sandY;
                resting = true;
            }
            transform.position = p;
            return;
        }

        restTimer += Time.deltaTime;
        if (restTimer >= restSeconds)
        {
            float t = (restTimer - restSeconds) / fadeSeconds;
            if (body != null)
            {
                Color c = body.color;
                c.a = 1f - t;
                body.color = c;
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }

    void OnEnable() => FoodManager.Register(this);
    void OnDisable() => FoodManager.Unregister(this);
}
