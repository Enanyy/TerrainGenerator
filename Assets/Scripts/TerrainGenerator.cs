using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {


	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;


	public Material mapMaterial;

	public int terrainSize = 4;

    public int lod;

	Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
	
	void Start() {

		textureSettings.ApplyToMaterial (mapMaterial);
		textureSettings.UpdateMeshHeights (mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);


        int currentChunkCoordX = Mathf.RoundToInt(transform.position.x / meshSettings.meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(transform.position.y / meshSettings.meshWorldSize);



        for (int yOffset = -terrainSize; yOffset <= terrainSize; yOffset++)
        {
            for (int xOffset = -terrainSize; xOffset <= terrainSize; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings,
                    detailLevels, transform, mapMaterial);
                mTerrainChunkDic.Add(viewedChunkCoord, newChunk);

                newChunk.Load();
            }
        }

    }

    void Update() {
        float distance = CameraManager.Instance.distance;

        int detailLevel = 0;

        for (int i = 0; i < detailLevels.Length; ++i)
        {
            if (distance < detailLevels[i].distance)
            {
                detailLevel = i; break;
            }
        }

        if (distance >= detailLevels[detailLevels.Length - 1].distance)
        {
            detailLevel = detailLevels[detailLevels.Length - 1].lod;
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

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

}

[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float distance;
}
