using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shop popup: buy a fish with coins and drop it into the tank. Tapping outside the panel closes it.
public class ShopModal : MonoBehaviour
{
    [Serializable]
    struct Offer
    {
        public Button button;
        public Fish prefab;
        public int price;
        public TMP_Text priceText;
    }

    [SerializeField] CanvasGroup group;
    // Full-screen dim behind the panel; tapping it closes the shop.
    [SerializeField] Button outsideButton;
    [SerializeField] RectTransform panel;
    [SerializeField] Offer[] offers;
    [SerializeField] Transform fishParent;
    [SerializeField] float fadeSeconds = 0.15f;
    [SerializeField] float popScale = 0.85f;

    [Header("Can't afford")]
    [SerializeField] float shakeSeconds = 0.35f;
    [SerializeField] float shakeDistance = 3f;
    [SerializeField] Color notEnoughColor = new Color(1f, 0.3f, 0.3f);

    [Header("Drop in")]
    // How far above the visible top a bought fish starts falling.
    [SerializeField] float spawnAboveTop = 0.15f;
    [SerializeField] Vector2 landMargin = new Vector2(0.1f, 0.1f);
    // Fish stops falling around the middle of the swim area, give or take this much.
    [SerializeField] float landSpread = 0.15f;

    Coroutine fade;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (outsideButton != null) outsideButton.onClick.AddListener(CloseByTap);
        if (offers != null)
        {
            for (int i = 0; i < offers.Length; i++)
            {
                int index = i;
                if (offers[i].priceText != null) offers[i].priceText.text = offers[i].price.ToString();
                if (offers[i].button != null) offers[i].button.onClick.AddListener(() => Buy(index));
            }
        }
        SetVisible(0f, false);
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        StartFade(true);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        StartFade(false);
    }

    // Closed by the player, not by a purchase: same click as putting a tool down.
    void CloseByTap()
    {
        if (!IsOpen) return;
        GameAudio.ToolClicked();
        Close();
    }

    void Buy(int index)
    {
        if (!IsOpen) return;
        Offer offer = offers[index];
        if (TankStats.Main == null || !TankStats.Main.TrySpendCoins(offer.price))
        {
            GameAudio.Fail();
            StartCoroutine(NotEnough(offer));
            return;
        }

        GameAudio.CoinCollected();
        Close();
        SpawnFish(offer.prefab);
    }

    void SpawnFish(Fish prefab)
    {
        if (prefab == null || SwimArea.Main == null) return;
        Rect bounds = SwimArea.Main.Bounds;
        float top = CameraFit.Main != null ? CameraFit.Main.VisibleRect.yMax : bounds.yMax;
        Vector2 land = SwimArea.Main.RandomPoint(landMargin);
        land.y = bounds.center.y + UnityEngine.Random.Range(-landSpread, landSpread);
        Fish fish = Instantiate(prefab, new Vector3(land.x, top + spawnAboveTop, 0f), Quaternion.identity, fishParent);
        if (fish.TryGetComponent(out FishWander wander)) wander.DropIn(land.y);
    }

    IEnumerator NotEnough(Offer offer)
    {
        Transform item = offer.button != null ? offer.button.transform : null;
        Vector3 home = item != null ? item.localPosition : Vector3.zero;
        Color priceColor = offer.priceText != null ? offer.priceText.color : Color.white;
        if (offer.priceText != null) offer.priceText.color = notEnoughColor;

        for (float t = 0f; t < shakeSeconds; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / shakeSeconds;
            if (item != null) item.localPosition = home + Vector3.right * Mathf.Sin(t * 60f) * shakeDistance * k;
            yield return null;
        }

        if (item != null) item.localPosition = home;
        if (offer.priceText != null) offer.priceText.color = priceColor;
    }

    void StartFade(bool open)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Fade(open));
    }

    IEnumerator Fade(bool open)
    {
        float from = group != null ? group.alpha : 0f;
        float to = open ? 1f : 0f;
        // Block taps as soon as it starts opening; stop blocking as soon as it starts closing.
        SetVisible(from, open);
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            float a = Mathf.Lerp(from, to, t / fadeSeconds);
            SetVisible(a, open);
            yield return null;
        }
        SetVisible(to, open);
        fade = null;
    }

    void SetVisible(float alpha, bool interactive)
    {
        if (group != null)
        {
            group.alpha = alpha;
            group.interactable = interactive;
            group.blocksRaycasts = interactive;
        }
        if (panel != null) panel.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, alpha);
    }
}
