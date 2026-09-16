using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Start menu drawn over the live tank. Dims the aquarium, hides the HUD, and hands control
// to the game when Play is pressed.
public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup menuGroup;
    [SerializeField] CanvasGroup hudGroup;
    [SerializeField] Button playButton;
    [SerializeField] Button quitButton;
    [SerializeField] float fadeSeconds = 0.25f;
    [SerializeField] float pressedScale = 0.9f;
    [SerializeField] float maxQuitDelay = 0.5f;

    void Awake()
    {
        GameState.InMenu = true;
        SetGroup(menuGroup, 1f, true);
        SetGroup(hudGroup, 0f, false);

        if (playButton != null)
        {
            playButton.onClick.AddListener(Play);
            AddPressFeedback(playButton);
        }
        if (quitButton != null)
        {
#if UNITY_IOS && !UNITY_EDITOR
            // iOS apps must not quit themselves.
            quitButton.gameObject.SetActive(false);
#else
            quitButton.onClick.AddListener(() => StartCoroutine(Quit()));
            AddPressFeedback(quitButton);
#endif
        }
    }

    void Play()
    {
        if (!GameState.InMenu) return;
        GameAudio.MenuButton();
        StartCoroutine(FadeToGame());
    }

    IEnumerator FadeToGame()
    {
        SetGroup(menuGroup, 1f, false);
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            float k = t / fadeSeconds;
            if (menuGroup != null) menuGroup.alpha = 1f - k;
            if (hudGroup != null) hudGroup.alpha = k;
            yield return null;
        }
        SetGroup(menuGroup, 0f, false);
        SetGroup(hudGroup, 1f, true);
        GameState.InMenu = false;
        if (menuGroup != null) menuGroup.gameObject.SetActive(false);
    }

    IEnumerator Quit()
    {
        SetGroup(menuGroup, 1f, false);
        // Let the click sound finish before the app closes.
        yield return new WaitForSecondsRealtime(Mathf.Min(GameAudio.MenuButton(), maxQuitDelay));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void SetGroup(CanvasGroup group, float alpha, bool interactive)
    {
        if (group == null) return;
        group.alpha = alpha;
        group.interactable = interactive;
        group.blocksRaycasts = interactive;
    }

    void AddPressFeedback(Button button)
    {
        if (!button.TryGetComponent(out EventTrigger trigger)) trigger = button.gameObject.AddComponent<EventTrigger>();
        Transform t = button.transform;
        AddEntry(trigger, EventTriggerType.PointerDown, () => t.localScale = Vector3.one * pressedScale);
        AddEntry(trigger, EventTriggerType.PointerUp, () => t.localScale = Vector3.one);
        AddEntry(trigger, EventTriggerType.PointerExit, () => t.localScale = Vector3.one);
    }

    static void AddEntry(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }
}
