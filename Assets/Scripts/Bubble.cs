using UnityEngine;

// A bubble rising from the sand. Grows through its sprites as it climbs, then fades out near the top.
[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    SpriteRenderer body;
    // Smallest first.
    Sprite[] sizes;
    float speed;
    float wobbleAmplitude;
    float wobbleFrequency;
    float phase;
    float baseX;
    float startY;
    float topY;
    // Fraction of the climb spent fading out at the end.
    const float FadePortion = 0.1f;

    public void Launch(Sprite[] growSprites, float riseSpeed, float wobble, float wobbleHz, float top)
    {
        body = GetComponent<SpriteRenderer>();
        sizes = growSprites;
        speed = riseSpeed;
        wobbleAmplitude = wobble;
        wobbleFrequency = wobbleHz;
        phase = Random.value * Mathf.PI * 2f;
        baseX = transform.position.x;
        startY = transform.position.y;
        topY = top;
        Refresh(0f);
    }

    void Update()
    {
        Vector3 p = transform.position;
        p.y += speed * Time.deltaTime;
        p.x = baseX + Mathf.Sin(Time.time * wobbleFrequency * Mathf.PI * 2f + phase) * wobbleAmplitude;
        transform.position = p;

        float t = Mathf.InverseLerp(startY, topY, p.y);
        Refresh(t);
        if (t >= 1f) Destroy(gameObject);
    }

    void Refresh(float t)
    {
        if (body == null || sizes == null || sizes.Length == 0) return;
        int index = Mathf.Min((int)(t * sizes.Length), sizes.Length - 1);
        body.sprite = sizes[index];

        Color c = body.color;
        c.a = Mathf.Clamp01((1f - t) / FadePortion);
        body.color = c;
    }
}
