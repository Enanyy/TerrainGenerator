using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
	public Vector2 coord;
	 
	private GameObject mMeshObject;
    private Vector2 mSampleCenter;


    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;

    private LODMesh[] mLODMeshes;

    private HeightMap mHeightMap;
    private bool mRequestingHeightMap;

    private TerrainGenerator mTerrain;

    private Dictionary<TreeSettings.TreeLayer, List<Matrix4x4>> mTreeMatrix4x4s = new Dictionary<TreeSettings.TreeLayer, List<Matrix4x4>>();

    private bool mGeneratingTree;
	public TerrainChunk(TerrainGenerator terrain, Vector2 coord)
    {
        this.mTerrain = terrain;

        this.mHeightMap = new HeightMap(mTerrain.meshSettings.numVertsPerLine, mTerrain.meshSettings.numVertsPerLine);

        SetCoord(coord);

		mLODMeshes = new LODMesh[terrain.lodSettings.detailLevels.Length];
		for (int i = 0; i < terrain.lodSettings.detailLevels.Length; i++) {
			mLODMeshes[i] = new LODMesh(terrain.lodSettings.detailLevels[i].lod);
            mLODMeshes[i].updateCallback += UpdateTerrainChunk;
        }
	}

    public void SetCoord(Vector2 coord)
    {
        this.coord = coord;

        mSampleCenter = coord * mTerrain.meshSettings.meshWorldSize / mTerrain.meshSettings.meshScale;
        Vector2 position = coord * mTerrain.meshSettings.meshWorldSize;

        if (mMeshObject == null)
        {
            mMeshObject = new GameObject("Terrain Chunk-" + this.coord.ToString());
            mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
            mMeshFilter = mMeshObject.AddComponent<MeshFilter>();
            mMeshRenderer.material = mTerrain.material;
            mMeshObject.transform.parent = mTerrain.transform;
        }
        else
        {
            mMeshObject.name = "Terrain Chunk-" + this.coord.ToString();
        }

        mMeshObject.transform.position = new Vector3(position.x, 0, position.y);

        if (mHeightMap.sampleCenter != mSampleCenter)
        {
            mMeshFilter.mesh = null;
            Load();
        }
    }

    public void Load()
    {
        if (mRequestingHeightMap == false)
        {
            mRequestingHeightMap = true;
            ThreadQueue.RunAsync(()=>mHeightMap.GenerateHeightMap(mTerrain.heightMapSettings, mSampleCenter),OnHeightMapReceived);
        }
    }



    void OnHeightMapReceived() {
		
		mRequestingHeightMap = false;

        if (mHeightMap.sampleCenter == mSampleCenter)
        {
            UpdateTerrainChunk();
        }
        else
        {
            Load();
        }
    }


    public void UpdateTerrainChunk()
    { 
        if (mHeightMap.sampleCenter == mSampleCenter)
        {
            int lod = mTerrain.lod;

            LODMesh lodMesh = mLODMeshes[lod];
            
            if (lodMesh.mesh != null && lodMesh.sampleCenter == mHeightMap.sampleCenter)
            {
                mMeshFilter.mesh = lodMesh.mesh;

                var matrix4x4 = mMeshObject.transform.localToWorldMatrix;

                ThreadQueue.RunAsync(()=> GenerateTree(matrix4x4, lod));
            }
            else
            {
                lodMesh.GenerateMesh(mHeightMap, mTerrain.meshSettings);
            }
        }
        else
        {
            Load();
        }
    }

    public void SetActive(bool active)
    {
        mMeshRenderer.enabled = active;
       
    }

   
    private void GenerateTree(Matrix4x4 matrix4X4, int lod)
    {
        mGeneratingTree = true;

        var it = mTreeMatrix4x4s.GetEnumerator();
        while(it.MoveNext())
        {
            it.Current.Value.Clear();
        }
        for (int k = 0; k < mTerrain.treeSettings.trees.Length; k++)
        {
            var layer = mTerrain.treeSettings.trees[k];

            float step = (layer.maxScale - layer.minScale) / mLODMeshes.Length ;

            float current = layer.minScale + lod * step * (1 + 1f / mLODMeshes.Length);

            System.Random random = new System.Random(layer.seed);

            for (int i = 0; i < mHeightMap.width; i += random.Next(1, layer.distance))
            {
                for (int j = 0; j < mHeightMap.height; j += random.Next(1, layer.distance))
                {
                    float y = mHeightMap.values[i, j] / mTerrain.heightMapSettings.heightMultiplier;

                    if (y > layer.minHeight && y < layer.maxHeight)
                    {

                        float x = i * mTerrain.meshSettings.meshScale - mTerrain.meshSettings.meshWorldSize / 2;
                        float z = -j * mTerrain.meshSettings.meshScale + mTerrain.meshSettings.meshWorldSize / 2;

                        float rx = random.Next(0, 100) / 100f;
                        float ry = random.Next(0, 100) / 100f;

                        Vector4 v4 = new Vector4(x + rx, y * mTerrain.heightMapSettings.heightMultiplier, z + ry, 1);
                        v4 = matrix4X4 * v4;

                        Vector3 position = new Vector3(v4.x,v4.y,v4.z);

                        var rotation = Quaternion.Euler(0, random.Next(0, 360), 0);

                        float scale = random.Next((int)(layer.minScale * 100), (int)(layer.maxScale * 100)) / 100f;
                        if (scale >= current)
                        {
                            if (mTreeMatrix4x4s.ContainsKey(layer) == false)
                            {
                                mTreeMatrix4x4s.Add(layer, new List<Matrix4x4>());
                            }
                            if (mTreeMatrix4x4s[layer].Count < 1023)
                            {
                                mTreeMatrix4x4s[layer].Add(Matrix4x4.TRS(position, rotation, Vector3.one * scale));
                            }
                        }
                    }
                }
            }
        }

        mGeneratingTree = false;
    }

    public void Update()
    {
        if(mTerrain.generateTree && mMeshRenderer.enabled && mGeneratingTree == false)
        {
            var it = mTreeMatrix4x4s.GetEnumerator();
            while(it.MoveNext())
            {

                Graphics.DrawMeshInstanced(it.Current.Key.mesh, 0, it.Current.Key.material, it.Current.Value);
            }
        }
    }
}

class LODMesh
{
    public Vector2 sampleCenter;
    public Mesh mesh;
    private bool mRequestingMesh;

	public int lod;
	public event System.Action updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(MeshData meshDataObject)
    {
		mesh = meshDataObject.CreateMesh ();
        mRequestingMesh = false;

		updateCallback ();
	}

    public void GenerateMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        if (mRequestingMesh == false || sampleCenter != heightMap.sampleCenter)
        {
            sampleCenter = heightMap.sampleCenter;
            mRequestingMesh = true;
            ThreadQueue.RunAsync(() => heightMap.GenerateMeshData(meshSettings,lod), OnMeshDataReceived);
        }
    }
}