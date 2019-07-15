using UnityEngine;

public class TerrainChunk
{

    public Vector2 coord;

    private GameObject mMeshObject;
    private Vector2 mSampleCentre;
    private Bounds mBounds;

    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;


    private LODInfo[] mDetailLevels;
    private LODMesh[] mLodMeshes;

    private HeightMap mHeightMap;
    private bool mHeightMapReceived;
    private int mPreviousLODIndex = -1;


    private HeightMapSettings mHeightMapSettings;
    private MeshSettings mMeshSettings;

    


    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings,
        LODInfo[] detailLevels,  Transform parent, Material material)
    {
        this.coord = coord;
        this.mDetailLevels = detailLevels;

        this.mHeightMapSettings = heightMapSettings;
        this.mMeshSettings = meshSettings;

        mSampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        mBounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);


        mMeshObject = new GameObject("Terrain Chunk-" + coord.ToString());
        mMeshRenderer = mMeshObject.AddComponent<MeshRenderer>();
        mMeshFilter = mMeshObject.AddComponent<MeshFilter>();
       
        mMeshRenderer.material = material;

        mMeshObject.transform.position = new Vector3(position.x, 0, position.y);
        mMeshObject.transform.parent = parent;


        mLodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            mLodMeshes[i] = new LODMesh(detailLevels[i].lod);
            mLodMeshes[i].updateCallback += UpdateTerrainChunk;
          
        }

       
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(mMeshSettings.numVertsPerLine, mMeshSettings.numVertsPerLine,
                mHeightMapSettings, mSampleCentre), OnHeightMapReceived);
    }



    void OnHeightMapReceived(object heightMapObject)
    {
        this.mHeightMap = (HeightMap) heightMapObject;
        mHeightMapReceived = true;

        UpdateTerrainChunk(0);
    }

   
    public void UpdateTerrainChunk(int lod)
    {
        
        if (mHeightMapReceived)
        {
            if (lod != mPreviousLODIndex)
            {
                LODMesh lodMesh = mLodMeshes[lod];
                if (lodMesh.hasMesh)
                {
                    mPreviousLODIndex = lod;
                    mMeshFilter.mesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mHeightMap, mMeshSettings);
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
    private int mLOD;
    public event System.Action<int> updateCallback;

    public LODMesh(int lod)
    {
        this.mLOD = lod;
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData) meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback(mLOD);
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, mLOD),
            OnMeshDataReceived);
    }

}