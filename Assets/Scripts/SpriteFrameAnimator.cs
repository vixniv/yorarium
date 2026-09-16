using UnityEngine;

// Loops a set of sprite frames on a SpriteRenderer. Each instance starts at a random point
// so a tank full of fish or plants doesn't flap in sync.
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer target;
    [SerializeField] Sprite[] frames;
    [SerializeField] float fps = 4f;
    [SerializeField] bool randomStart = true;

    float time;
    int shownIndex = -1;

    public SpriteRenderer Target => target;

    void Awake()
    {
        if (target == null) target = GetComponent<SpriteRenderer>();
        if (randomStart && frames != null && frames.Length > 0) time = Random.value * frames.Length / Mathf.Max(fps, 0.01f);
    }

    void OnEnable() => Show(true);

    public void Setup(SpriteRenderer renderer, Sprite[] newFrames, float framesPerSecond)
    {
        target = renderer;
        fps = framesPerSecond;
        if (randomStart && newFrames != null && newFrames.Length > 0) time = Random.value * newFrames.Length / Mathf.Max(fps, 0.01f);
        SetFrames(newFrames);
    }

    // Swap to another animation (e.g. fish growing up) without restarting the timing.
    public void SetFrames(Sprite[] newFrames)
    {
        frames = newFrames;
        Show(true);
    }

    void Update()
    {
        time += Time.deltaTime;
        Show(false);
    }

    void Show(bool force)
    {
        if (target == null || frames == null || frames.Length == 0) return;
        int index = (int)(time * fps) % frames.Length;
        if (!force && index == shownIndex) return;
        shownIndex = index;
        if (frames[index] != null) target.sprite = frames[index];
    }
}
