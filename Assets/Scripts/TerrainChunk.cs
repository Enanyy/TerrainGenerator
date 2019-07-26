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

    private Dictionary<TreeSettings.TreeLayer, List<GameObject>> mTrees = new Dictionary<TreeSettings.TreeLayer, List<GameObject>>();

	public TerrainChunk(TerrainGenerator terrain, Vector2 coord)
    {
        this.mTerrain = terrain;

        this.mHeightMap = new HeightMap(mTerrain.meshSettings.numVertsPerLine, mTerrain.meshSettings.numVertsPerLine);

        SetCoord(coord);

		mLODMeshes = new LODMesh[terrain.detailLevels.Length];
		for (int i = 0; i < terrain.detailLevels.Length; i++) {
			mLODMeshes[i] = new LODMesh(terrain.detailLevels[i].lod);
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
            ThreadQueue.DoAction(()=>mHeightMap.GenerateHeightMap(mTerrain.heightMapSettings, mSampleCenter),OnHeightMapReceived);
        }
    }



    void OnHeightMapReceived() {
		
		mRequestingHeightMap = false;

        if (mHeightMap.sampleCenter == mSampleCenter)
        {
            UpdateTerrainChunk(mTerrain.lod);
        }
        else
        {
            Load();
        }
    }


    public void UpdateTerrainChunk(int lod)
    { 
        if (mHeightMap.sampleCenter == mSampleCenter)
        {
            LODMesh lodMesh = mLODMeshes[lod];
            
            if (lodMesh.mesh != null && lodMesh.sampleCenter == mHeightMap.sampleCenter)
            {
                mMeshFilter.mesh = lodMesh.mesh;
                GenerateTree();
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
        if(active == false)
        {
            HideTree();
        }
    }


    private void HideTree()
    {
        var it = mTrees.GetEnumerator();
        while (it.MoveNext())
        {
            var list = it.Current.Value;
            for (int i = 0; i < list.Count; ++i)
            {
                it.Current.Key.ReturnTree(list[i]);
            }
            list.Clear();
        }

    }

    private void GenerateTree()
    {
        HideTree();


        for (int k = 0; k < mTerrain.treeSettings.trees.Length; k++)
        {
            var layer = mTerrain.treeSettings.trees[k];

            Random.InitState(layer.seed);

            for (int i = 0; i < mHeightMap.width; i += Random.Range(1, layer.distance))
            {
                for (int j = 0; j < mHeightMap.height; j += Random.Range(1, layer.distance))
                {
                    float y = mHeightMap.values[i, j];

                    if (y > layer.minHeight && y < layer.maxHeight)
                    {

                        float x = i * mTerrain.meshSettings.meshScale - mTerrain.meshSettings.meshWorldSize / 2;
                        float z = -j * mTerrain.meshSettings.meshScale + mTerrain.meshSettings.meshWorldSize / 2;

                        Vector2 r = Random.insideUnitCircle * layer.range;

                        Vector3 position = new Vector3(x + r.x, y, z + r.y);

                        GameObject go = layer.InstantiateTree();
                        go.transform.SetParent(mMeshObject.transform);
                        go.transform.localPosition = position;

                        go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                        if(mTrees.ContainsKey(layer) ==false)
                        {
                            mTrees.Add(layer, new List<GameObject>());
                        }
                        mTrees[layer].Add(go);
                    }
                }
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
	public event System.Action<int> updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject)
    {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
        mRequestingMesh = false;

		updateCallback (lod);
	}

    public void GenerateMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        if (mRequestingMesh == false || sampleCenter != heightMap.sampleCenter)
        {
            sampleCenter = heightMap.sampleCenter;
            mRequestingMesh = true;
            ThreadQueue.DoFunc(() => heightMap.GenerateMeshData(meshSettings,lod), OnMeshDataReceived);
        }
    }
}