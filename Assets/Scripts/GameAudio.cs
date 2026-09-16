using UnityEngine;

// One place for gameplay sound effects.
[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    [SerializeField] AudioClip coinCollected;
    [SerializeField] AudioClip coinPop;
    [SerializeField] AudioClip feeding;
    [SerializeField] AudioClip fishEat;
    [SerializeField] AudioClip menuButton;
    [SerializeField] AudioClip toolClicked;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    static GameAudio main;
    AudioSource source;

    void Awake()
    {
        main = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public static void CoinCollected() => Play(main != null ? main.coinCollected : null);
    public static void CoinPop() => Play(main != null ? main.coinPop : null);
    public static void Feeding() => Play(main != null ? main.feeding : null);
    public static void FishEat() => Play(main != null ? main.fishEat : null);
    public static void ToolClicked() => Play(main != null ? main.toolClicked : null);

    // Returns the clip length so callers can wait for it (e.g. before quitting).
    public static float MenuButton()
    {
        AudioClip clip = main != null ? main.menuButton : null;
        Play(clip);
        return clip != null ? clip.length : 0f;
    }

    static void Play(AudioClip clip)
    {
        if (clip == null) return;
        main.source.PlayOneShot(clip, main.volume);
    }
}
