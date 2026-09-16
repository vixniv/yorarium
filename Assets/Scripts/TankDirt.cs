using UnityEngine;

// Swaps the tank art through the dirty stages as cleanliness drops, and tints the tank green.
public class TankDirt : MonoBehaviour
{
    [SerializeField] SpriteFrameAnimator animator;
    [SerializeField] Sprite[] cleanFrames;
    [SerializeField] Sprite[] dirty1Frames;
    [SerializeField] Sprite[] dirty2Frames;
    [SerializeField] Sprite[] dirty3Frames;
    [SerializeField] SpriteRenderer tint;
    [SerializeField] Color tintColor = new Color(0.25f, 0.8f, 0.2f);
    // Tint opacity for clean, dirty1, dirty2 and dirty3.
    [SerializeField] float[] tintAlpha = { 0f, 0.08f, 0.16f, 0.25f };
    // Alpha change per second when moving between stages.
    [SerializeField] float fadeSpeed = 0.5f;

    int stage = -1;
    float alpha;

    void Awake()
    {
        if (animator == null) animator = GetComponent<SpriteFrameAnimator>();
    }

    void Start()
    {
        Refresh();
        alpha = StageAlpha();
        ApplyTint();
    }

    void Update()
    {
        Refresh();
        alpha = Mathf.MoveTowards(alpha, StageAlpha(), fadeSpeed * Time.deltaTime);
        ApplyTint();
    }

    void Refresh()
    {
        float c = TankStats.Main != null ? TankStats.Main.Cleanliness : 1f;
        int next = c > 0.75f ? 0 : c > 0.5f ? 1 : c > 0.25f ? 2 : 3;
        if (next == stage) return;
        stage = next;
        Sprite[] frames = stage switch { 0 => cleanFrames, 1 => dirty1Frames, 2 => dirty2Frames, _ => dirty3Frames };
        if (animator != null && frames != null && frames.Length > 0) animator.SetFrames(frames);
    }

    float StageAlpha() => tintAlpha != null && stage >= 0 && stage < tintAlpha.Length ? tintAlpha[stage] : 0f;

    void ApplyTint()
    {
        if (tint == null) return;
        Color c = tintColor;
        c.a = alpha;
        tint.color = c;
    }
}
