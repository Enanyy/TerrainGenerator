﻿using System;
using UnityEngine;

[System.Serializable]
public struct TerrainColor
{
    public string name;
    public float height;
    public Color color;
}

[Serializable]
public class TerrainSettings
{
    public int terrainChunkSize = 241;

    public float terrainChunkScale = 5f;

    public LODInfo[] detailLevels;

    public Material material;

    public int chunkSize = 4;

    public TerrainColor[] colors;

    public TextureSettings textureSettings;
}

