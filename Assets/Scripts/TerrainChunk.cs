using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainChunk
{

   private  GameObject meshObject;
   public Vector2 coord;

   private MeshRenderer meshRenderer;
   private MeshFilter meshFilter;
   private LODMesh[] lodMeshes;

   private TerrainData mapData;
   private bool mapDataReceived;

   private TerrainSettings settings;

    public TerrainChunk(Vector2 coord,Transform parent,TerrainSettings settings)
    {
        this.settings = settings;

        int size = settings.terrainChunkSize - 1;
        this.coord = coord * size;


        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer.material = settings.material;

        meshObject.transform.position = new Vector3(this.coord.x, 0, this.coord.y) * settings.terrainChunkScale;
        meshObject.transform.parent = parent;
        meshObject.transform.localScale = Vector3.one * settings.terrainChunkScale;


        lodMeshes = new LODMesh[settings.detailLevels.Length];
        for (int i = 0; i < settings.detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(settings.detailLevels[i].lod, UpdateTerrainChunk);
        }
        ThreadedDataRequester.RequestData(()=>TerrainGenerator.Instance.GenerateTerrainData(this.coord),OnTerrainDataReceived);
    }

    void OnTerrainDataReceived(object mapData)
    {
        this.mapData =(TerrainData) mapData;
        mapDataReceived = true;

        Texture2D texture = TextureGenerator.TextureFromColourMap(this.mapData.colourMap,
            settings.terrainChunkSize,
            settings.terrainChunkSize);
        meshRenderer.material.mainTexture = texture;

        UpdateTerrainChunk(TerrainGenerator.Instance.lod);
    }



    public void UpdateTerrainChunk( int lod)
    {
        if (mapDataReceived)
        {
            LODMesh lodMesh = lodMeshes[lod];
            if (lodMesh.hasMesh)
            {
                meshFilter.mesh = lodMesh.mesh;
            }
            else if (!lodMesh.hasRequestedMesh)
            {
                lodMesh.RequestMesh(mapData);
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
    System.Action<int> updateCallback;

    public LODMesh(int lod, System.Action<int> updateCallback)
    {
        this.lod = lod;
        this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(object meshData)
    {
        mesh = ((MeshData)meshData).CreateMesh();
        hasMesh = true;

        updateCallback(lod);
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
    public float distance;
}


