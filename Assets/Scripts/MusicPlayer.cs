using UnityEngine;

// Background theme. Starts part-way into the track and loops back to that same point.
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip theme;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;
    // Seconds into the clip where playback starts and every loop restarts.
    [SerializeField] float startTime = 18f;

    AudioSource source;
    bool appPaused;
    bool wasPlaying;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }

    void Start() => PlayFromStart();

    void Update()
    {
        // Streaming clips can report not-playing while buffering, so only restart
        // once the track was actually heard and then stopped at its end.
        if (source.isPlaying)
        {
            wasPlaying = true;
            return;
        }
        if (wasPlaying && !appPaused && theme != null)
        {
            wasPlaying = false;
            PlayFromStart();
        }
    }

    void OnApplicationPause(bool paused) => appPaused = paused;

    void PlayFromStart()
    {
        if (theme == null) return;
        source.clip = theme;
        source.volume = volume;
        source.Play();
        source.time = Mathf.Clamp(startTime, 0f, theme.length - 0.1f);
    }
}
