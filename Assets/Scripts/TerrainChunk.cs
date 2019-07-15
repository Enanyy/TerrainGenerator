using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainChunk
{

   private  GameObject meshObject;
   private Vector2 position;
   private Bounds bounds;

   private MeshRenderer meshRenderer;
   private MeshFilter meshFilter;

   private LODInfo[] detailLevels;
   private LODMesh[] lodMeshes;

   private TerrainData mapData;
   private bool mapDataReceived;
   private int previousLODIndex = -1;

    public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
    {
        this.detailLevels = detailLevels;

        position = coord * size;
        bounds = new Bounds(position, Vector2.one * size);
        Vector3 positionV3 = new Vector3(position.x, 0, position.y);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer.material = material;

        meshObject.transform.position = positionV3 * TerrainGenerator.Instance.terrainChunkScale;
        meshObject.transform.parent = parent;
        meshObject.transform.localScale = Vector3.one * TerrainGenerator.Instance.terrainChunkScale;


        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
        }

        //TerrainGenerator.Instance.RequestMapData(position, OnTerrainDataReceived);

        ThreadedDataRequester.RequestData(()=>TerrainGenerator.Instance.GenerateTerrainData(position),OnTerrainDataReceived);
    }

    void OnTerrainDataReceived(object mapData)
    {
        this.mapData =(TerrainData) mapData;
        mapDataReceived = true;

        Texture2D texture = TextureGenerator.TextureFromColourMap(this.mapData.colourMap, TerrainGenerator.terrainChunkSize,
            TerrainGenerator.terrainChunkSize);
        meshRenderer.material.mainTexture = texture;

        UpdateTerrainChunk();
    }



    public void UpdateTerrainChunk()
    {
        if (mapDataReceived)
        {
            int lodIndex = 0;
            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mapData);
                }
            }
        }
    }
}

class LODMesh
{

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback)
    {
        this.lod = lod;
        this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(object meshData)
    {
        mesh = ((MeshData)meshData).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(TerrainData mapData)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(()=>TerrainGenerator.Instance.GenerateTerrainMesh(mapData.heightMap,lod), OnMeshDataReceived);
       
    }

}

[System.Serializable]
public struct LODInfo
{
    public int lod;
    public float visibleDstThreshold;
}


