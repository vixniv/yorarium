using UnityEngine;
using UnityEngine.UI;

// Bottom-right tools. Food can and brush are selectable, one at a time (tap again to put down).
// The shop button opens the shop instead of being selected, and wiggles while the shop is open.
public class ToolBar : MonoBehaviour
{
    [SerializeField] Button foodCanButton;
    [SerializeField] Button brushButton;
    [SerializeField] Button shopButton;
    [SerializeField] ShopModal shopModal;
    [SerializeField] float selectedScale = 1.25f;
    // Selected tool rocks side to side continuously, like being shaken.
    [SerializeField] float wiggleAngle = 15f;
    // Full left-right swings per second.
    [SerializeField] float wiggleFrequency = 2f;

    Button selected;
    float wiggleTime;
    bool shopWiggling;
    float shopWiggleTime;

    public bool FeedMode => selected != null && selected == foodCanButton;
    public bool BrushMode => selected != null && selected == brushButton;

    void Awake()
    {
        if (foodCanButton != null) foodCanButton.onClick.AddListener(() => Toggle(foodCanButton));
        if (brushButton != null) brushButton.onClick.AddListener(() => Toggle(brushButton));
        if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
        Refresh();
    }

    void Update()
    {
        if (selected != null)
        {
            wiggleTime += Time.unscaledDeltaTime;
            Wiggle(selected, wiggleTime);
        }

        bool shopOpen = shopModal != null && shopModal.IsOpen;
        if (shopOpen != shopWiggling)
        {
            shopWiggling = shopOpen;
            shopWiggleTime = 0f;
            ShowShopState();
        }
        if (shopWiggling)
        {
            shopWiggleTime += Time.unscaledDeltaTime;
            Wiggle(shopButton, shopWiggleTime);
        }
    }

    void Wiggle(Button tool, float time)
    {
        if (tool == null) return;
        float angle = Mathf.Sin(time * wiggleFrequency * Mathf.PI * 2f) * wiggleAngle;
        tool.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Toggle(Button tool)
    {
        selected = selected == tool ? null : tool;
        wiggleTime = 0f;
        GameAudio.ToolClicked();
        Refresh();
    }

    void OpenShop()
    {
        GameAudio.ToolClicked();
        if (shopModal != null) shopModal.Open();
    }

    void Refresh()
    {
        ShowState(foodCanButton);
        ShowState(brushButton);
    }

    void ShowShopState()
    {
        if (shopButton == null) return;
        shopButton.transform.localScale = Vector3.one * (shopWiggling ? selectedScale : 1f);
        shopButton.transform.localRotation = Quaternion.identity;
    }

    void ShowState(Button tool)
    {
        if (tool == null) return;
        tool.transform.localScale = Vector3.one * (tool == selected ? selectedScale : 1f);
        tool.transform.localRotation = Quaternion.identity;
    }
}
