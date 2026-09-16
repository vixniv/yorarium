using System.Collections.Generic;
using UnityEngine;

// Hunts food, grows into an adult after enough meals, and drops a coin per meal.
[RequireComponent(typeof(FishWander))]
public class Fish : MonoBehaviour
{
    [SerializeField] Sprite[] smallFrames;
    [SerializeField] Sprite[] adultFrames;
    [SerializeField] int feedsToGrow = 3;
    [SerializeField] int feedCount;
    [SerializeField] Coin coinPrefab;
    // Pause between eating and the coin popping out.
    [SerializeField] float coinDelay = 0.8f;
    [SerializeField] float chaseSpeedMultiplier = 2.2f;
    [SerializeField] float eatDistance = 0.04f;
    // Rest time after a meal so one fish can't eat everything.
    [SerializeField] float mealCooldown = 1.5f;

    static readonly List<Fish> all = new List<Fish>();

    FishWander wander;
    SpriteFrameAnimator animator;
    Food prey;
    float cooldown;
    float lastMealTime = float.NegativeInfinity;

    public static IReadOnlyList<Fish> All => all;

    public bool IsAdult => feedCount >= feedsToGrow;
    // Not resting after a meal and not already chasing food.
    public bool IsFree => cooldown <= 0f && prey == null;
    public float TimeSinceMeal => Time.time - lastMealTime;

    void Awake()
    {
        wander = GetComponent<FishWander>();
        animator = GetComponentInChildren<SpriteFrameAnimator>();
    }

    void OnEnable() => all.Add(this);

    void Start() => ApplySprite();

    // Called by FoodManager.
    public void AssignPrey(Food food)
    {
        prey = food;
        food.ClaimedBy = this;
    }

    public void ReleasePrey()
    {
        if (prey != null && prey.ClaimedBy == this) prey.ClaimedBy = null;
        prey = null;
        wander.ClearTarget();
    }

    void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        if (prey == null) return;

        // Food meter filled up mid-chase: lose interest right away.
        if (TankStats.Main != null && TankStats.Main.IsFoodFull)
        {
            ReleasePrey();
            return;
        }

        Vector2 target = prey.transform.position;
        if (Vector2.Distance(transform.position, target) <= eatDistance)
        {
            Eat();
            return;
        }
        wander.SetTarget(target, chaseSpeedMultiplier);
    }

    void LateUpdate()
    {
        // Pellet faded away before we reached it.
        if (prey == null) wander.ClearTarget();
    }

    void Eat()
    {
        Destroy(prey.gameObject);
        prey = null;
        wander.ClearTarget();
        cooldown = mealCooldown;
        lastMealTime = Time.time;

        bool wasAdult = IsAdult;
        feedCount++;
        if (!wasAdult && IsAdult) ApplySprite();

        // Overfeeding gives nothing: no coin while the food meter is already full.
        bool meterFull = TankStats.Main != null && TankStats.Main.IsFoodFull;
        GameAudio.FishEat();
        if (coinPrefab != null && !meterFull) StartCoroutine(DropCoinAfterDelay());
        if (TankStats.Main != null) TankStats.Main.OnFishFed();
    }

    System.Collections.IEnumerator DropCoinAfterDelay()
    {
        yield return new WaitForSeconds(coinDelay);
        // Spawn where the fish is now, since it keeps swimming during the delay.
        Instantiate(coinPrefab, transform.position, Quaternion.identity);
        GameAudio.CoinPop();
    }

    void ApplySprite()
    {
        Sprite[] frames = IsAdult ? adultFrames : smallFrames;
        if (frames == null || frames.Length == 0) return;
        if (animator != null) animator.SetFrames(frames);
        else if (wander.Body != null) wander.Body.sprite = frames[0];
    }

    void OnDisable()
    {
        all.Remove(this);
        if (prey != null && prey.ClaimedBy == this) prey.ClaimedBy = null;
        prey = null;
    }
}
