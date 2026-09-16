using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows TankStats on the HUD: food meter fill, health (cleanliness) fill and coin count.
public class HudView : MonoBehaviour
{
    [SerializeField] Image foodFill;
    [SerializeField] Image healthFill;
    [SerializeField] TMP_Text coinText;

    TankStats stats;

    void OnEnable()
    {
        stats = TankStats.Main != null ? TankStats.Main : FindAnyObjectByType<TankStats>();
        if (stats == null) return;
        stats.CoinsChanged += ShowCoins;
        stats.FoodChanged += ShowFood;
        stats.CleanlinessChanged += ShowHealth;
        ShowCoins(stats.Coins);
        ShowFood(stats.Food);
        ShowHealth(stats.Cleanliness);
    }

    void OnDisable()
    {
        if (stats == null) return;
        stats.CoinsChanged -= ShowCoins;
        stats.FoodChanged -= ShowFood;
        stats.CleanlinessChanged -= ShowHealth;
    }

    void ShowCoins(int coins)
    {
        if (coinText != null) coinText.text = coins.ToString();
    }

    void ShowFood(float food)
    {
        if (foodFill != null) foodFill.fillAmount = food;
    }

    void ShowHealth(float health)
    {
        if (healthFill != null) healthFill.fillAmount = health;
    }
}
