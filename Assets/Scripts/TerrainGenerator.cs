using UnityEngine;

using System.Collections.Generic;


public class TerrainGenerator : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 25f;

    const float sqrViewerMoveThresholdForChunkUpdate =
        viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;


    public Material mapMaterial;

    public int chunkGridSize = 4;

    private float mMeshWorldSize;
    
    private Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();

    public int lod = 0;
   
    void Start()
    {

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

       
        mMeshWorldSize = meshSettings.meshWorldSize;
       
        int coordX = Mathf.RoundToInt(transform.position.x / mMeshWorldSize);
        int coordY = Mathf.RoundToInt(transform.position.y / mMeshWorldSize);

        for (int yOffset = -chunkGridSize; yOffset <= chunkGridSize; yOffset++)
        {
            for (int xOffset = -chunkGridSize; xOffset <= chunkGridSize; xOffset++)
            {
                Vector2 coord = new Vector2(coordX + xOffset, coordY + yOffset);

                TerrainChunk chunk = new TerrainChunk(coord, heightMapSettings, meshSettings,
                    detailLevels, transform, mapMaterial);
                mTerrainChunkDic.Add(coord, chunk);

                chunk.Load();

            }
        }
    }

    void Update()
    {
        int lodIndex = 0;
        float distance = CameraManager.Instance.distance;

        for (int i = 0; i < detailLevels.Length; i++)
        {
            if (distance < detailLevels[i].distance)
            {
                lodIndex = i;
                break;   
            }      
        }

        if (lod != lodIndex)
        {
            lod = lodIndex;

            var it = mTerrainChunkDic.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.UpdateTerrainChunk(lodIndex);
            }
        }
    }
}

[System.Serializable]
public struct LODInfo
{
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float distance;
}
