using System;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    //public static HeightMap GenerateHeightMap(int mapWidth, 
    //    int mapHeight,
    //    int seed,
    //    float scale,
    //    int octaves,
    //    float persistance,
    //    float lacunarity,
    //    Vector2 offset,
    //    Noise.NormalizeMode normalizeMode)
    //{

    //}
}

public struct HeightMap
{
    public readonly float[,] heightMap;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] heightMap, float minValue, float maxValue)
    {
        this.heightMap = heightMap;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}

