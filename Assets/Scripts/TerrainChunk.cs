using UnityEngine;

public class TerrainChunk {
	

	public Vector2 coord;
	 
	private GameObject mMeshObject;
    private Vector2 mSampleCenter;


    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;

    private LODMesh[] mLODMeshes;

    private HeightMap mHeightMap;
    private bool mRequestingHeightMap;

    private HeightMapSettings mHeightMapSettings;
    private MeshSettings mMeshSettings;

    private TerrainGenerator mTerrain;

	public TerrainChunk(TerrainGenerator terrain, Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels)
    {
        this.mTerrain = terrain;

		this.mHeightMapSettings = heightMapSettings;
		this.mMeshSettings = meshSettings;
        this.mHeightMap = new HeightMap(mMeshSettings.numVertsPerLine,mMeshSettings.numVertsPerLine);

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
            ThreadQueue.DoAction(()=>mHeightMap.GenerateHeightMap(mHeightMapSettings,mSampleCenter),OnHeightMapReceived);
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
            }
            else
            {
                lodMesh.GenerateMesh(mHeightMap, mMeshSettings);
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