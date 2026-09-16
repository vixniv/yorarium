using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Taps in the tank: collect coins first, otherwise drop food when the food can is selected.
public class TankInput : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] ToolBar toolBar;
    // Extra reach around the tap so small coins are easy to hit with a finger.
    [SerializeField] float tapRadius = 0.06f;

    readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    PointerEventData uiPointer;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (GameState.InMenu) return;

        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Vector2 screen = pointer.position.ReadValue();
        if (IsOverUI(screen)) return;

        Vector2 world = cam.ScreenToWorldPoint(screen);

        Collider2D hit = Physics2D.OverlapCircle(world, tapRadius);
        if (hit != null && hit.TryGetComponent(out Coin coin))
        {
            coin.Collect();
            return;
        }

        if (toolBar != null && toolBar.FeedMode && FoodManager.Main != null && SwimArea.Main != null)
        {
            if (SwimArea.Main.Bounds.Contains(world) && FoodManager.Main.Spawn(world))
                GameAudio.Feeding();
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
