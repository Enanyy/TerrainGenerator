using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainChunk
{

   private  GameObject mMeshObject;
   public Vector2 coord;

   private MeshRenderer mMeshRenderer;
   private MeshFilter mMeshFilter;
   private LODMesh[] mLODMeshes;

   private TerrainData mTerrainData;
   private bool mReceivedTerrainData;

   private TerrainSettings mSettings;

    public TerrainChunk(Vector2 coord,Transform parent,TerrainSettings settings)
    {
        this.mSettings = settings;

        int size = settings.terrainChunkSize - 1;
        this.coord = coord * size;


        mMeshObject = new GameObject("Terrain Chunk");
        mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
        mMeshFilter = mMeshObject.AddComponent<MeshFilter>();

        if (settings.terrainType == TerrainSettings.TerrainType.Color)
        {
            mMeshRenderer.material = settings.colorSettings.material;
        }
        else
        {
            mMeshRenderer.material = settings.textureSettings.material;
        }


        mMeshObject.transform.position = new Vector3(this.coord.x, 0, this.coord.y) * settings.terrainChunkScale;
        mMeshObject.transform.parent = parent;
        mMeshObject.transform.localScale = Vector3.one * settings.terrainChunkScale;


        mLODMeshes = new LODMesh[settings.detailLevels.Length];
        for (int i = 0; i < settings.detailLevels.Length; i++)
        {
            mLODMeshes[i] = new LODMesh(settings.detailLevels[i].lod, OnMeshDataReceived);
        }
        ThreadedDataRequester.RequestData(()=>TerrainGenerator.GenerateTerrainData(this.coord, settings),OnTerrainDataReceived);
    }

    void OnMeshDataReceived(LODMesh mesh)
    {
        UpdateTerrainChunk(mesh.lod);
        if (mSettings.terrainType == TerrainSettings.TerrainType.Texture)
        {
            mSettings.textureSettings.UpdateMeshHeights(mesh.meshData.minHeight, mesh.meshData.maxHeight);
        }
    }

    void OnTerrainDataReceived(object mapData)
    {
        this.mTerrainData =(TerrainData) mapData;
        mReceivedTerrainData = true;

        if (mSettings.terrainType == TerrainSettings.TerrainType.Color)
        {
            Texture2D texture = TextureGenerator.TextureFromColourMap(this.mTerrainData.colourMap,
                mSettings.terrainChunkSize,
                mSettings.terrainChunkSize);
            mMeshRenderer.material.mainTexture = texture;
        }

        UpdateTerrainChunk(TerrainGenerator.Instance.lod);
    }



    public void UpdateTerrainChunk( int lod)
    {
        if (mReceivedTerrainData)
        {
            LODMesh lodMesh = mLODMeshes[lod];
            if (lodMesh.hasMesh)
            {
                mMeshFilter.mesh = lodMesh.mesh;
            }
            else if (!lodMesh.hasRequestedMesh)
            {
                lodMesh.RequestMesh(mTerrainData, mSettings);
            }
        }
    }

    void OnMeshDataReceived(MeshData meshData)
    {

    }
}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    public int lod;
    System.Action<LODMesh> updateCallback;
   
    public MeshData meshData;

    public LODMesh(int lod, System.Action<LODMesh> updateCallback)
    {
        this.lod = lod;
        this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(object meshData)
    {
        this.meshData = (MeshData)meshData;
        mesh = this.meshData.CreateMesh();
        hasMesh = true;

        updateCallback(this);

       
    }

    public void RequestMesh(TerrainData mapData,TerrainSettings terrainSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(()=>MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainSettings,lod), OnMeshDataReceived); 
    }

}

[System.Serializable]
public struct LODInfo
{
    public int lod;
    public float distance;
}


