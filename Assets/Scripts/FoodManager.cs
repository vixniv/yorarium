using System.Collections.Generic;
using UnityEngine;

// Spawns food and decides which fish goes for which pellet.
// Pairs closest fish with food first, favours fish that haven't eaten for a while,
// and hands a pellet over when a free fish is much closer than the one chasing it.
public class FoodManager : MonoBehaviour
{
    [SerializeField] Food foodPrefab;
    [SerializeField] int maxFood = 8;
    [SerializeField] float assignInterval = 0.2f;

    [Header("Hunger priority")]
    // Distance (world units) a fully hungry fish is allowed to "win" by over a just-fed one.
    [SerializeField] float hungerBonus = 0.25f;
    // Seconds without food until a fish counts as fully hungry.
    [SerializeField] float secondsToFullHunger = 10f;

    [Header("Hand-over")]
    // A free fish takes over when it is closer than this fraction of the chaser's distance.
    [SerializeField, Range(0.1f, 1f)] float switchRatio = 0.5f;
    // Chasers this close keep their pellet no matter what.
    [SerializeField] float keepDistance = 0.15f;

    static readonly List<Food> active = new List<Food>();

    struct Pair
    {
        public Fish fish;
        public Food food;
        public float score;
    }

    readonly List<Pair> pairs = new List<Pair>();
    float timer;

    public static FoodManager Main { get; private set; }

    void Awake() => Main = this;

    public static void Register(Food food) => active.Add(food);
    public static void Unregister(Food food) => active.Remove(food);

    public bool Spawn(Vector2 position)
    {
        if (foodPrefab == null || active.Count >= maxFood) return false;
        Instantiate(foodPrefab, position, Quaternion.identity, transform);
        Assign();
        return true;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = assignInterval;
        Assign();
    }

    void Assign()
    {
        if (active.Count == 0) return;
        if (TankStats.Main != null && TankStats.Main.IsFoodFull)
        {
            // Full fish ignore food: drop any chase in progress and leave pellets to sink.
            ReleaseAll();
            return;
        }
        HandOverToCloserFish();
        PairFreeFishWithFood();
    }

    void HandOverToCloserFish()
    {
        foreach (Food food in active)
        {
            Fish owner = food.ClaimedBy;
            if (owner == null) continue;

            float ownerDistance = Distance(owner, food);
            if (ownerDistance <= keepDistance) continue;

            Fish closer = null;
            float closerDistance = ownerDistance * switchRatio;
            foreach (Fish fish in Fish.All)
            {
                if (!fish.IsFree) continue;
                float d = Distance(fish, food);
                if (d < closerDistance)
                {
                    closer = fish;
                    closerDistance = d;
                }
            }

            if (closer == null) continue;
            owner.ReleasePrey();
            closer.AssignPrey(food);
        }
    }

    void PairFreeFishWithFood()
    {
        pairs.Clear();
        foreach (Fish fish in Fish.All)
        {
            if (!fish.IsFree) continue;
            float hunger = Mathf.Clamp01(fish.TimeSinceMeal / secondsToFullHunger);
            foreach (Food food in active)
            {
                if (food.ClaimedBy != null) continue;
                pairs.Add(new Pair
                {
                    fish = fish,
                    food = food,
                    score = Distance(fish, food) - hunger * hungerBonus,
                });
            }
        }
        if (pairs.Count == 0) return;

        // Best (lowest) scores first; each fish and pellet used once.
        pairs.Sort((a, b) => a.score.CompareTo(b.score));
        foreach (Pair p in pairs)
        {
            if (!p.fish.IsFree || p.food.ClaimedBy != null) continue;
            p.fish.AssignPrey(p.food);
        }
    }

    static void ReleaseAll()
    {
        foreach (Fish fish in Fish.All)
            if (!fish.IsFree) fish.ReleasePrey();
    }

    static float Distance(Fish fish, Food food) =>
        Vector2.Distance(fish.transform.position, food.transform.position);
}
