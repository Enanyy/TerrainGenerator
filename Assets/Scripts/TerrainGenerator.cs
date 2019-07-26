using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {


	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureSettings textureSettings;


	public Material material;

	public int terrainSizeX = 4;
    public int terrainSizeY = 4;

    public int lod;

    private Dictionary<Vector2, TerrainChunk> mTerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
    private Dictionary<Vector2, TerrainChunk> mTerrainChunkDic2 = new Dictionary<Vector2, TerrainChunk>();
    private Queue<TerrainChunk> mCacheChunkList = new Queue<TerrainChunk>();

	void Start()
    {
        CameraManager.Instance.onZoom -= OnZoom;
        CameraManager.Instance.onZoom += OnZoom;
        CameraManager.Instance.onMove -= OnMove;
        CameraManager.Instance.onMove += OnMove;

        textureSettings.ApplyToMaterial (material);
		textureSettings.UpdateMeshHeights (material, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        UpdateViewChunk();
    }

    void UpdateViewChunk()
    {

        float distance = Mathf.Abs(CameraManager.Instance.leftTop.x - CameraManager.Instance.rightTop.x);

        int chunkSize = (int)(distance / meshSettings.meshWorldSize);
        if (distance % meshSettings.meshWorldSize > 0) chunkSize++;

        terrainSizeX = chunkSize / 2 + 1;

        //distance = Mathf.Abs(CameraManager.Instance.leftTop.z - CameraManager.Instance.leftBottom.z);

        //chunkSize = (int)(distance / meshSettings.meshWorldSize);
        //if (distance % meshSettings.meshWorldSize > 0) chunkSize++;

        //terrainSizeY = chunkSize / 2 + 2;

        terrainSizeY = terrainSizeX - (terrainSizeX - 1) / 2;

        Vector3 center = CameraManager.Instance.center;

        int currentChunkCoordX = Mathf.RoundToInt(center.x / meshSettings.meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(center.z / meshSettings.meshWorldSize);

        mTerrainChunkDic2.Clear();

        for (int yOffset = -terrainSizeY; yOffset <= terrainSizeY; yOffset++)
        {
            for (int xOffset = -terrainSizeX; xOffset <= terrainSizeX; xOffset++)
            {
                Vector2 coord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                TerrainChunk chunk = null;
                mTerrainChunkDic.TryGetValue(coord, out chunk);
                if (chunk != null)
                {
                   mTerrainChunkDic2.Add(coord, chunk);
                   mTerrainChunkDic.Remove(coord);
                }
                else
                {
                    if (mCacheChunkList.Count > 0)
                    {
                        chunk = mCacheChunkList.Dequeue();
                        chunk.SetCoord(coord);
                        chunk.SetActive(true);
                    }
                    else
                    {
                        chunk = new TerrainChunk(this, coord, heightMapSettings, meshSettings, detailLevels);
                    }

                    mTerrainChunkDic2.Add(coord, chunk);

                    chunk.Load();
                }
            }
        }

        var tmp = mTerrainChunkDic;
        mTerrainChunkDic = mTerrainChunkDic2;
        mTerrainChunkDic2 = tmp;

        var it = mTerrainChunkDic2.GetEnumerator();
        while (it.MoveNext())
        {
            it.Current.Value.SetActive(false);
            mCacheChunkList.Enqueue(it.Current.Value);
        }
        mTerrainChunkDic2.Clear();

    }

    void OnMove()
    {
        UpdateViewChunk();
    }

    void OnZoom(float distance)
    {
        UpdateViewChunk();

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

        textureSettings.ApplyToMaterial(material);
        textureSettings.UpdateMeshHeights(material, heightMapSettings.minHeight, heightMapSettings.maxHeight);


    }

    private void Update()
    {

        ThreadQueue.Update();
    }
}

[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float distance;
}
