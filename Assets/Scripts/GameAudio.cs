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
    [SerializeField] AudioClip fail;
    // One recording holding several brush strokes; each range (start, end seconds) is played on its own.
    [SerializeField] AudioClip cleaning;
    [SerializeField] Vector2[] cleaningStrokes =
    {
        new Vector2(0f, 0.52f), new Vector2(1.02f, 1.51f), new Vector2(1.97f, 2.53f), new Vector2(3f, 3.52f),
        new Vector2(3.95f, 4.48f), new Vector2(4.93f, 5.42f), new Vector2(5.8f, 6.32f),
    };
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    static GameAudio main;
    AudioSource source;
    AudioClip[] strokeClips;
    int lastStroke = -1;

    void Awake()
    {
        main = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        strokeClips = Slice(cleaning, cleaningStrokes);
    }

    public static void CoinCollected() => Play(main != null ? main.coinCollected : null);
    public static void CoinPop() => Play(main != null ? main.coinPop : null);
    public static void Feeding() => Play(main != null ? main.feeding : null);
    public static void FishEat() => Play(main != null ? main.fishEat : null);
    public static void ToolClicked() => Play(main != null ? main.toolClicked : null);
    public static void Fail() => Play(main != null ? main.fail : null);

    // A random brush stroke, never the same one twice in a row.
    public static void Cleaning()
    {
        if (main == null || main.strokeClips == null || main.strokeClips.Length == 0) return;
        int count = main.strokeClips.Length;
        int index = Random.Range(0, count);
        if (count > 1 && index == main.lastStroke) index = (index + 1 + Random.Range(0, count - 1)) % count;
        main.lastStroke = index;
        Play(main.strokeClips[index]);
    }

    // Returns the clip length so callers can wait for it (e.g. before quitting).
    public static float MenuButton()
    {
        AudioClip clip = main != null ? main.menuButton : null;
        Play(clip);
        return clip != null ? clip.length : 0f;
    }

    static AudioClip[] Slice(AudioClip clip, Vector2[] ranges)
    {
        if (clip == null || ranges == null || ranges.Length == 0) return null;
        // Falls back to the whole clip when its samples can't be read (e.g. streamed import).
        if (clip.loadType != AudioClipLoadType.DecompressOnLoad || !clip.LoadAudioData()) return new[] { clip };

        int channels = clip.channels;
        var all = new float[clip.samples * channels];
        if (!clip.GetData(all, 0)) return new[] { clip };

        var clips = new AudioClip[ranges.Length];
        for (int i = 0; i < ranges.Length; i++)
        {
            int start = Mathf.Clamp(Mathf.RoundToInt(ranges[i].x * clip.frequency), 0, clip.samples - 1);
            int end = Mathf.Clamp(Mathf.RoundToInt(ranges[i].y * clip.frequency), start + 1, clip.samples);
            var part = new float[(end - start) * channels];
            System.Array.Copy(all, start * channels, part, 0, part.Length);
            clips[i] = AudioClip.Create($"{clip.name}_{i}", end - start, channels, clip.frequency, false);
            clips[i].SetData(part, 0);
        }
        return clips;
    }

    static void Play(AudioClip clip)
    {
        if (clip == null) return;
        main.source.PlayOneShot(clip, main.volume);
    }
}
