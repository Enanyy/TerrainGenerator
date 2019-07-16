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
    private bool mHeightMapReceived;


    private HeightMapSettings mHeightMapSettings;
    private MeshSettings mMeshSettings;


	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels,  Transform parent,  Material material) {
		this.coord = coord;
		this.mDetailLevels = detailLevels;

		this.mHeightMapSettings = heightMapSettings;
		this.mMeshSettings = meshSettings;


		mSampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		

		mMeshObject = new GameObject("Terrain Chunk");
		mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
		mMeshFilter = mMeshObject.AddComponent<MeshFilter>();
		
		mMeshRenderer.material = material;

		mMeshObject.transform.position = new Vector3(position.x,0,position.y);
		mMeshObject.transform.parent = parent;


		mLODMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			mLODMeshes[i] = new LODMesh(detailLevels[i].lod);
			mLODMeshes[i].updateCallback += UpdateTerrainChunk;
			
		}

	}

	public void Load() {
		ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap (mMeshSettings.numVertsPerLine, mMeshSettings.numVertsPerLine, mHeightMapSettings, mSampleCenter), OnHeightMapReceived);
	}



	void OnHeightMapReceived(object heightMapObject) {
		this.mHeightMap = (HeightMap)heightMapObject;
		mHeightMapReceived = true;

		UpdateTerrainChunk (0);
	}


    public void UpdateTerrainChunk(int lod)
    {
        if (mHeightMapReceived)
        {
            LODMesh lodMesh = mLODMeshes[lod];
            if (lodMesh.hasMesh)
            {
                mMeshFilter.mesh = lodMesh.mesh;
            }
            else if (!lodMesh.hasRequestedMesh)
            {
                lodMesh.RequestMesh(mHeightMap, mMeshSettings);
            }
        }
    }
}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	int lod;
	public event System.Action<int> updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
		hasMesh = true;

		updateCallback (lod);
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData (() => MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, lod), OnMeshDataReceived);
	}

}