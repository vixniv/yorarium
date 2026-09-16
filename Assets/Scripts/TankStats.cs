using System;
using UnityEngine;

// Tank-wide numbers shown on the HUD: coins, the food meter and tank cleanliness (health).
public class TankStats : MonoBehaviour
{
    [SerializeField] int coins;
    [SerializeField, Range(0f, 1f)] float food = 0.5f;
    // Seconds for a full food meter to drain to empty.
    [SerializeField] float secondsToEmpty = 120f;
    [SerializeField] float foodPerMeal = 0.1f;
    // The meter drains every frame, so it rarely sits at exactly 1. Anything above this counts as full.
    [SerializeField, Range(0f, 1f)] float fullThreshold = 0.95f;
    [SerializeField, Range(0f, 1f)] float cleanliness = 1f;
    // Seconds for a spotless tank to get fully dirty.
    [SerializeField] float secondsToDirty = 180f;

    public static TankStats Main { get; private set; }

    public event Action<int> CoinsChanged;
    public event Action<float> FoodChanged;
    public event Action<float> CleanlinessChanged;

    public int Coins => coins;
    public float Food => food;
    public float Cleanliness => cleanliness;
    public bool IsFoodFull => food >= fullThreshold;

    void Awake() => Main = this;

    void Update()
    {
        if (GameState.InMenu) return;
        if (food > 0f) SetFood(food - Time.deltaTime / secondsToEmpty);
        if (cleanliness > 0f) SetCleanliness(cleanliness - Time.deltaTime / secondsToDirty);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        CoinsChanged?.Invoke(coins);
    }

    // Takes the coins only if there are enough.
    public bool TrySpendCoins(int amount)
    {
        if (coins < amount) return false;
        AddCoins(-amount);
        return true;
    }

    public void OnFishFed() => SetFood(food + foodPerMeal);

    public void Clean(float amount) => SetCleanliness(cleanliness + amount);

    void SetFood(float value)
    {
        food = Mathf.Clamp01(value);
        FoodChanged?.Invoke(food);
    }

    void SetCleanliness(float value)
    {
        cleanliness = Mathf.Clamp01(value);
        CleanlinessChanged?.Invoke(cleanliness);
    }
}
