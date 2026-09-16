using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Places a random set of plants along the sand each time the scene starts.
public class PlantScatter : MonoBehaviour
{
    [Serializable]
    public struct PlantArt
    {
        public Sprite[] frames;
    }

    [SerializeField] PlantArt[] plants;
    [SerializeField] float fps = 2f;
    [SerializeField] Vector2Int countRange = new Vector2Int(3, 5);
    [SerializeField] Vector2 scaleRange = new Vector2(1.4f, 1.8f);
    [SerializeField] Vector2 xRange = new Vector2(-0.55f, 0.55f);
    // Base height; plants lower on the sand are drawn in front.
    [SerializeField] Vector2 yRange = new Vector2(-0.96f, -0.88f);
    // How far a plant may drift from the middle of its slot (0 = evenly spaced, 0.5 = anywhere in slot).
    [SerializeField, Range(0f, 0.5f)] float jitter = 0.3f;
    [SerializeField] int sortingOrder = 1;

    void Start()
    {
        if (plants == null || plants.Length == 0) return;

        // Even mix: cycle through every sprite (random start), then shuffle positions.
        int count = Mathf.Max(Random.Range(countRange.x, countRange.y + 1), plants.Length);
        int start = Random.Range(0, plants.Length);
        var picks = new List<PlantArt>(count);
        for (int i = 0; i < count; i++) picks.Add(plants[(start + i) % plants.Length]);
        for (int i = picks.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (picks[i], picks[j]) = (picks[j], picks[i]);
        }

        // One slot per plant across the sand so they never pile up.
        float slot = (xRange.y - xRange.x) / count;
        for (int i = 0; i < count; i++)
        {
            float x = xRange.x + slot * (i + 0.5f + Random.Range(-jitter, jitter));
            Spawn(picks[i], x);
        }
    }

    void Spawn(PlantArt art, float x)
    {
        if (art.frames == null || art.frames.Length == 0 || art.frames[0] == null) return;
        float y = Random.Range(yRange.x, yRange.y);

        var plant = new GameObject(art.frames[0].name);
        plant.transform.SetParent(transform, false);
        plant.transform.localPosition = new Vector3(x, y, 0f);
        plant.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

        var sr = plant.AddComponent<SpriteRenderer>();
        sr.sprite = art.frames[0];
        sr.flipX = Random.value < 0.5f;
        // Lower plants in front: map y range to 0..3 on top of the base order (stays behind fish, food and coins).
        float depth = Mathf.InverseLerp(yRange.y, yRange.x, y);
        sr.sortingOrder = sortingOrder + Mathf.RoundToInt(depth * 3f);

        plant.AddComponent<SpriteFrameAnimator>().Setup(sr, art.frames, fps);
    }
}
