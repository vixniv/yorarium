using UnityEngine;
using UnityEngine.UI;

// Bottom-right tools: food can, brush and shop. One can be selected at a time (tap again to put down);
// only the food can does something so far.
public class ToolBar : MonoBehaviour
{
    [SerializeField] Button foodCanButton;
    [SerializeField] Button brushButton;
    [SerializeField] Button shopButton;
    [SerializeField] float selectedScale = 1.25f;
    // Selected tool rocks side to side continuously, like being shaken.
    [SerializeField] float wiggleAngle = 15f;
    // Full left-right swings per second.
    [SerializeField] float wiggleFrequency = 2f;

    Button selected;
    float wiggleTime;

    public bool FeedMode => selected != null && selected == foodCanButton;
    public bool BrushMode => selected != null && selected == brushButton;
    public bool ShopMode => selected != null && selected == shopButton;

    void Awake()
    {
        if (foodCanButton != null) foodCanButton.onClick.AddListener(() => Toggle(foodCanButton));
        if (brushButton != null) brushButton.onClick.AddListener(() => Toggle(brushButton));
        if (shopButton != null) shopButton.onClick.AddListener(() => Toggle(shopButton));
        Refresh();
    }

    void Update()
    {
        if (selected == null) return;
        wiggleTime += Time.unscaledDeltaTime;
        float angle = Mathf.Sin(wiggleTime * wiggleFrequency * Mathf.PI * 2f) * wiggleAngle;
        selected.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Toggle(Button tool)
    {
        selected = selected == tool ? null : tool;
        wiggleTime = 0f;
        GameAudio.ToolClicked();
        Refresh();
    }

    void Refresh()
    {
        ShowState(foodCanButton);
        ShowState(brushButton);
        ShowState(shopButton);
    }

    void ShowState(Button tool)
    {
        if (tool == null) return;
        tool.transform.localScale = Vector3.one * (tool == selected ? selectedScale : 1f);
        tool.transform.localRotation = Quaternion.identity;
    }
}
