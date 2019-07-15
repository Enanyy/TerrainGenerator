using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode mode = Noise.NormalizeMode.Global;
    public float noiseScale = 20;

    [Range(0, 100)]
    public int octaves = 8;
    [Range(0, 1)]
    public float persistance= 0.5f;
    [Range(1,10)]
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

}
[Serializable]
public class HeightMapSettings
{
    public float meshHeightMultiplier = 10;
    public AnimationCurve meshHeightCurve;

}

[Serializable]
public class TerrainSettings
{
    public int terrainChunkSize = 241;

    public float terrainChunkScale = 5f;

    public LODInfo[] detailLevels;

    public Material material;

    public int chunkSize = 4;
}

public class TerrainGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	
	[Range(0,6)]
	public int editorPreviewLOD;

    public NoiseSettings noiseSettings;
    public HeightMapSettings heightMapSettings;
    public TerrainSettings terrainSettings;
   
	
	public bool autoUpdate;

    public TerrainType[] regions;
    

    public static TerrainGenerator Instance;
  
    public int lod = 0;

    Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int yOffset = -terrainSettings.chunkSize; yOffset <= terrainSettings.chunkSize; yOffset++)
        {
            for (int xOffset = -terrainSettings.chunkSize; xOffset <= terrainSettings.chunkSize; xOffset++)
            {
                Vector2 coord = new Vector2(xOffset, yOffset);


                mTerrainChunkDic.Add(coord, new TerrainChunk(coord, transform,terrainSettings));
            }
        }
    }

    public void DrawMapInEditor() {
		TerrainData mapData = GenerateTerrainData (Vector2.zero);

		TerrainDisplay display = FindObjectOfType<TerrainDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap, heightMapSettings, editorPreviewLOD), TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize));
		}
	}



	void Update()
    {
        float distance = CameraManager.Instance.distance;

        int detailLevel = 0;

        for (int i = 0; i < terrainSettings.detailLevels.Length; ++i)
        {
            if (distance < terrainSettings.detailLevels[i].distance)
            {
                detailLevel = i;break;
            }
        }

        if (distance >= terrainSettings.detailLevels[terrainSettings.detailLevels.Length - 1].distance)
        {
            detailLevel = terrainSettings.detailLevels[terrainSettings.detailLevels.Length - 1].lod;
        }

        if (detailLevel != lod)
        {
            lod = detailLevel;
            var it = mTerrainChunkDic.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.UpdateTerrainChunk(lod);
            }
        }

    }

    public TerrainData GenerateTerrainData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseData(terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize, noiseSettings,centre);

        Color[] colorMap = new Color[terrainSettings.terrainChunkSize * terrainSettings.terrainChunkSize];
        for (int y = 0; y < terrainSettings.terrainChunkSize; y++)
        {
            for (int x = 0; x < terrainSettings.terrainChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * terrainSettings.terrainChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        return new TerrainData(noiseMap, colorMap);
    }

    public MeshData GenerateTerrainMesh(float[,] heightMap, int lod)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, heightMapSettings, lod);
        return meshData;
    }

}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}

public struct TerrainData {
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

	public TerrainData (float[,] heightMap, Color[] colourMap)
	{
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}
