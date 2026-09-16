using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Taps in the tank: collect coins first, otherwise drop food when the food can is selected.
// With the brush selected, swiping across the tank scrubs it clean.
public class TankInput : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] ToolBar toolBar;
    // Extra reach around the tap so small coins are easy to hit with a finger.
    [SerializeField] float tapRadius = 0.06f;

    [Header("Brush")]
    // Cleanliness gained per world unit of swipe.
    [SerializeField] float cleanPerUnit = 0.15f;
    // Swipe distance that counts as one stroke (one scrub sound).
    [SerializeField] float strokeDistance = 0.25f;
    // Minimum seconds between scrub sounds.
    [SerializeField] float strokeCooldown = 0.3f;

    readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    PointerEventData uiPointer;

    bool brushing;
    Vector2 lastBrushPoint;
    float strokeTravel;
    float lastStrokeTime = float.NegativeInfinity;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (GameState.InMenu)
        {
            brushing = false;
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (brushing) Brush(pointer);
        if (!pointer.press.wasPressedThisFrame) return;

        Vector2 screen = pointer.position.ReadValue();
        if (IsOverUI(screen)) return;

        Vector2 world = cam.ScreenToWorldPoint(screen);

        Collider2D hit = Physics2D.OverlapCircle(world, tapRadius);
        if (hit != null && hit.TryGetComponent(out Coin coin))
        {
            coin.Collect();
            return;
        }

        if (toolBar != null && toolBar.BrushMode)
        {
            brushing = true;
            lastBrushPoint = world;
            strokeTravel = 0f;
            return;
        }

        if (toolBar != null && toolBar.FeedMode && FoodManager.Main != null && SwimArea.Main != null)
        {
            if (SwimArea.Main.Bounds.Contains(world) && FoodManager.Main.Spawn(world))
                GameAudio.Feeding();
        }
    }

    void Brush(Pointer pointer)
    {
        if (!pointer.press.isPressed || toolBar == null || !toolBar.BrushMode)
        {
            brushing = false;
            return;
        }

        Vector2 world = cam.ScreenToWorldPoint(pointer.position.ReadValue());
        float moved = Vector2.Distance(world, lastBrushPoint);
        lastBrushPoint = world;
        if (moved <= 0f) return;

        if (TankStats.Main != null) TankStats.Main.Clean(moved * cleanPerUnit);

        strokeTravel += moved;
        if (strokeTravel >= strokeDistance && Time.unscaledTime - lastStrokeTime >= strokeCooldown)
        {
            strokeTravel = 0f;
            lastStrokeTime = Time.unscaledTime;
            GameAudio.Cleaning();
        }
    }

    bool IsOverUI(Vector2 screen)
    {
        EventSystem es = EventSystem.current;
        if (es == null) return false;
        uiPointer ??= new PointerEventData(es);
        uiPointer.position = screen;
        uiHits.Clear();
        es.RaycastAll(uiPointer, uiHits);
        return uiHits.Count > 0;
    }
}
