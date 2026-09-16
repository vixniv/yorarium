using UnityEngine;

// Slow drifting jellyfish: wanders mostly vertically with a gentle bob.
public class JellyDrift : MonoBehaviour
{
    [SerializeField] SwimArea area;
    [SerializeField] float speed = 0.08f;
    [SerializeField] float bobAmplitude = 0.02f;
    [SerializeField] float bobFrequency = 1.2f;
    [SerializeField] Vector2 margin = new Vector2(0.12f, 0.12f);

    Vector2 basePosition;
    Vector2 target;
    float phase;

    void Start()
    {
        if (area == null) area = SwimArea.Main;
        basePosition = transform.position;
        phase = Random.value * Mathf.PI * 2f;
        PickTarget();
    }

    void Update()
    {
        if (area == null) return;

        basePosition = Vector2.MoveTowards(basePosition, target, speed * Time.deltaTime);
        if (Vector2.Distance(basePosition, target) < 0.01f) PickTarget();

        float bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f + phase) * bobAmplitude;
        transform.position = new Vector3(basePosition.x, basePosition.y + bob, transform.position.z);
    }

    void PickTarget()
    {
        Vector2 p = area.RandomPoint(margin);
        // Keep horizontal moves small so it mostly rises and sinks.
        p.x = Mathf.Lerp(basePosition.x, p.x, 0.35f);
        target = p;
    }
}
