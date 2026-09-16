using UnityEngine;

// Releases bubbles from the sand now and then, sometimes a small puff of a few at once.
public class BubbleSpawner : MonoBehaviour
{
    // Smallest to largest; a bubble grows through them as it rises.
    [SerializeField] Sprite[] sprites;
    [SerializeField] Vector2 intervalRange = new Vector2(4f, 8f);
    [SerializeField, Range(0f, 1f)] float puffChance = 0.1f;
    [SerializeField] Vector2Int puffCount = new Vector2Int(2, 3);
    [SerializeField] int maxBubbles = 3;
    [SerializeField] Vector2 speedRange = new Vector2(0.12f, 0.2f);
    [SerializeField] float wobbleAmplitude = 0.01f;
    [SerializeField] Vector2 wobbleFrequencyRange = new Vector2(0.8f, 1.6f);
    // How far above the sand bubbles appear from.
    [SerializeField] float sandOffset = 0.02f;
    [SerializeField] int sortingOrder = 5;

    float timer;

    void Start() => timer = Random.Range(0f, intervalRange.y);

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Random.Range(intervalRange.x, intervalRange.y);

        if (sprites == null || sprites.Length == 0 || SwimArea.Main == null) return;

        Rect area = SwimArea.Main.Bounds;
        float x = Random.Range(area.xMin, area.xMax);
        int count = Random.value < puffChance ? Random.Range(puffCount.x, puffCount.y + 1) : 1;
        for (int i = 0; i < count && transform.childCount < maxBubbles; i++)
            Spawn(x + Random.Range(-0.02f, 0.02f), SwimArea.Main.SandY + sandOffset - i * 0.03f);
    }

    void Spawn(float x, float y)
    {
        // Pop out at the top of the visible water.
        float top = CameraFit.Main != null ? CameraFit.Main.VisibleRect.yMax : SwimArea.Main.Bounds.yMax;

        var go = new GameObject("Bubble");
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        go.AddComponent<Bubble>().Launch(
            sprites,
            Random.Range(speedRange.x, speedRange.y),
            wobbleAmplitude,
            Random.Range(wobbleFrequencyRange.x, wobbleFrequencyRange.y),
            top);
    }
}
