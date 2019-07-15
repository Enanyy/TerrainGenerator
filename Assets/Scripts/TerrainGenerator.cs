using UnityEngine;
using System;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	
	[Range(0,6)]
	public int editorPreviewLOD;

    public TerrainSettings terrainSettings;
   
	
	public bool autoUpdate;

    public static TerrainGenerator Instance;
  
    public int lod = 0;

    Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        terrainSettings.textureSettings.ApplyToMaterial();

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
		TerrainData mapData = GenerateTerrainData (Vector2.zero,terrainSettings);

		TerrainDisplay display = FindObjectOfType<TerrainDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap, terrainSettings, editorPreviewLOD), TextureGenerator.TextureFromColourMap (mapData.colourMap, terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize));
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

    public static TerrainData GenerateTerrainData(Vector2 centre,TerrainSettings terrainSettings)
    {
        float[,] noiseMap = Noise.GenerateNoiseData(terrainSettings.terrainChunkSize, terrainSettings.terrainChunkSize, terrainSettings.heightMapSettings.noiseSettings,centre);

        Color[] colorMap = new Color[terrainSettings.terrainChunkSize * terrainSettings.terrainChunkSize];
        for (int y = 0; y < terrainSettings.terrainChunkSize; y++)
        {
            for (int x = 0; x < terrainSettings.terrainChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainSettings.colorSettings.colors.Length; i++)
                {
                    if (currentHeight >= terrainSettings.colorSettings.colors[i].height)
                    {
                        colorMap[y * terrainSettings.terrainChunkSize + x] = terrainSettings.colorSettings.colors[i].color;
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
