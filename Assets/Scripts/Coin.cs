using UnityEngine;

// Coin dropped by a fed fish. Sinks to the sand and waits to be tapped.
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] float sinkSpeed = 0.2f;
    [SerializeField] float restJitter = 0.04f;
    [SerializeField] int value = 1;

    float restY;

    void Start()
    {
        float sandY = SwimArea.Main != null ? SwimArea.Main.SandY : -1f;
        restY = sandY - Random.Range(0f, restJitter);
    }

    void Update()
    {
        Vector3 p = transform.position;
        if (p.y <= restY) return;
        p.y = Mathf.Max(restY, p.y - sinkSpeed * Time.deltaTime);
        transform.position = p;
    }

    public void Collect()
    {
        if (TankStats.Main != null) TankStats.Main.AddCoins(value);
        GameAudio.CoinCollected();
        Destroy(gameObject);
    }
}
