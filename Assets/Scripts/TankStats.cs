using System;
using UnityEngine;

// Tank-wide numbers shown on the HUD: coins and the food meter.
public class TankStats : MonoBehaviour
{
    [SerializeField] int coins;
    [SerializeField, Range(0f, 1f)] float food = 0.5f;
    // Seconds for a full food meter to drain to empty.
    [SerializeField] float secondsToEmpty = 120f;
    [SerializeField] float foodPerMeal = 0.1f;
    // The meter drains every frame, so it rarely sits at exactly 1. Anything above this counts as full.
    [SerializeField, Range(0f, 1f)] float fullThreshold = 0.95f;

    public static TankStats Main { get; private set; }

    public event Action<int> CoinsChanged;
    public event Action<float> FoodChanged;

    public int Coins => coins;
    public float Food => food;
    public bool IsFoodFull => food >= fullThreshold;

    void Awake() => Main = this;

    void Update()
    {
        if (food <= 0f || GameState.InMenu) return;
        SetFood(food - Time.deltaTime / secondsToEmpty);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        CoinsChanged?.Invoke(coins);
    }

    public void OnFishFed() => SetFood(food + foodPerMeal);

    void SetFood(float value)
    {
        food = Mathf.Clamp01(value);
        FoodChanged?.Invoke(food);
    }
}
