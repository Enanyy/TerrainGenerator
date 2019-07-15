using System;
using UnityEngine;

[Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode mode = Noise.NormalizeMode.Global;
    public float noiseScale = 20;

    [Range(0, 100)]
    public int octaves = 8;
    [Range(0, 1)]
    public float persistance = 0.5f;
    [Range(1, 10)]
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

}
