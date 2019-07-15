using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	public Noise.NormalizeMode normalizeMode;

	public const int terrainChunkSize = 241;
	[Range(0,6)]
	public int editorPreviewLOD;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	Queue<MapThreadInfo<TerrainData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<TerrainData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private static TerrainGenerator mInstance;

    public static TerrainGenerator Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<TerrainGenerator>();
            }

            return mInstance;
        }
    }


    public float terrainChunkScale = 5f;


    public LODInfo[] detailLevels;

    public Material material;

    public int chunkSize = 4;


    Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();

    void Start()
    {


        for (int yOffset = -chunkSize; yOffset <= chunkSize; yOffset++)
        {
            for (int xOffset = -chunkSize; xOffset <= chunkSize; xOffset++)
            {
                Vector2 coord = new Vector2(xOffset, yOffset);


                mTerrainChunkDic.Add(coord, new TerrainChunk(coord, TerrainGenerator.terrainChunkSize - 1, detailLevels, transform, material));
            }
        }
    }

    public void DrawMapInEditor() {
		TerrainData mapData = GenerateTerrainData (Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainChunkSize, terrainChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainChunkSize, terrainChunkSize));
		}
	}



	void Update() {

	}

    public TerrainData GenerateTerrainData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseData(terrainChunkSize, terrainChunkSize, seed, noiseScale, octaves,
            persistance, lacunarity, centre + offset, normalizeMode);

        Color[] colourMap = new Color[terrainChunkSize * terrainChunkSize];
        for (int y = 0; y < terrainChunkSize; y++)
        {
            for (int x = 0; x < terrainChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * terrainChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        return new TerrainData(noiseMap, colourMap);
    }

    public MeshData GenerateTerrainMesh(float[,] heightMap, int lod)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        return meshData;
    }

    void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

	struct MapThreadInfo<T> {
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

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
