using System;
using UnityEngine;

[System.Serializable]
public struct TerrainColor
{
    public string name;
    [Range(0,1)]
    public float height;
    public Color color;
}

[Serializable]
public class TerrainSettings
{
    public int terrainChunkSize = 241;

    public float terrainChunkScale = 5f;

    public LODInfo[] detailLevels;

    public int chunkSize = 4;

    public TerrainColorSettings colorSettings;

    public TerrainTextureSettings textureSettings;
}

