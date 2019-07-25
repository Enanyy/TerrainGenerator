using UnityEngine;

public class TerrainChunk {
	

	public Vector2 coord;
	 
	private GameObject mMeshObject;
    private Vector2 mSampleCenter;


    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;


    private LODInfo[] mDetailLevels;
    private LODMesh[] mLODMeshes;


    private HeightMap mHeightMap;
    private bool mRequestingHeightMap;


    private HeightMapSettings mHeightMapSettings;
    private MeshSettings mMeshSettings;

    private TerrainGenerator mTerrain;

	public TerrainChunk(TerrainGenerator terrain, Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels)
    {
        this.mTerrain = terrain;

		this.mDetailLevels = detailLevels;

		this.mHeightMapSettings = heightMapSettings;
		this.mMeshSettings = meshSettings;

        SetCoord(coord);

		mLODMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			mLODMeshes[i] = new LODMesh(detailLevels[i].lod);
            mLODMeshes[i].updateCallback += UpdateTerrainChunk;
        }
	}

    public void SetCoord(Vector2 coord)
    {
        this.coord = coord;

        mSampleCenter = coord * mMeshSettings.meshWorldSize / mMeshSettings.meshScale;
        Vector2 position = coord * mMeshSettings.meshWorldSize;

        if (mMeshObject == null)
        {
            mMeshObject = new GameObject("Terrain Chunk-" + this.coord.ToString());
            mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
            mMeshFilter = mMeshObject.AddComponent<MeshFilter>();
        }
        else
        {
            mMeshObject.name = "Terrain Chunk-" + this.coord.ToString();
        }

        mMeshRenderer.material = mTerrain.material;

        mMeshObject.transform.position = new Vector3(position.x, 0, position.y);
        mMeshObject.transform.parent = mTerrain.transform;


        if (mHeightMap != null && mHeightMap.sampleCenter != mSampleCenter)
        {
            Load();
        }
    }

    public void Load()
    {
        if (mRequestingHeightMap == false)
        {
            mRequestingHeightMap = true;
            ThreadedDataRequester.RequestData(
                () => HeightMapGenerator.GenerateHeightMap(mMeshSettings.numVertsPerLine, mMeshSettings.numVertsPerLine,
                    mHeightMapSettings, mSampleCenter), OnHeightMapReceived);
        }
    }



    void OnHeightMapReceived(object heightMapObject) {
		this.mHeightMap = (HeightMap)heightMapObject;
		mRequestingHeightMap = false;

		UpdateTerrainChunk (mTerrain.lod);
	}


    public void UpdateTerrainChunk(int lod)
    {
        if (mHeightMap == null)
        {
            return;
        }

        if (mHeightMap.sampleCenter == mSampleCenter)
        {
            LODMesh lodMesh = mLODMeshes[lod];
            
            if (lodMesh.mesh != null && lodMesh.heightMap.sampleCenter == mHeightMap.sampleCenter)
            {
                mMeshFilter.mesh = lodMesh.mesh;
            }
            else
            {
                lodMesh.RequestMesh(mHeightMap, mMeshSettings);
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
}

class LODMesh
{
    public HeightMap heightMap;
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

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        if (mRequestingMesh == false 
            || this.heightMap == null                        
            || this.heightMap.sampleCenter != heightMap.sampleCenter)
        {
            this.heightMap = heightMap;
            mRequestingMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
        }
    }

}