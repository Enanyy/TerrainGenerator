using UnityEngine;
using System.Collections;

public class TerrainPreview : MonoBehaviour
{

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;


    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    };

    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureSettings;
    public TreeSettings treeSettings;

    public Material terrainMaterial;

    private HeightMap mHeightMap;


    [Range(0, LODSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    public bool generateTree = true;


    public bool autoUpdate;




    public void DrawMapInEditor()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        mHeightMap = new HeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine);

        if (drawMode == DrawMode.NoiseMap)
        {
            mHeightMap.GenerateHeightMap(heightMapSettings, Vector2.zero);
            DrawTexture(mHeightMap.GenerateTexture());
        }
        else if (drawMode == DrawMode.Mesh)
        {
            mHeightMap.GenerateHeightMap(heightMapSettings, Vector2.zero);
            DrawMesh(mHeightMap.GenerateMeshData(meshSettings, editorPreviewLOD));
           
                GenerateTree();
            

        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            mHeightMap.minValue = 0;
            mHeightMap.maxValue = 1;
            mHeightMap.GenerateFalloffMap();
            DrawTexture(mHeightMap.GenerateTexture());
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }



    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureSettings != null)
        {
            textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }

        if (treeSettings != null)
        {
            treeSettings.OnValuesUpdated -= OnValuesUpdated;
            treeSettings.OnValuesUpdated += OnValuesUpdated;
        }

    }

    private void GenerateTree()
    {
        int count = meshRenderer.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            var child = meshRenderer.transform.GetChild(i);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        if (generateTree == false)
        {
            return;
        }

        for (int k = 0; k < treeSettings.trees.Length; k++)
        {
            var layer = treeSettings.trees[k];

            float step = (layer.maxScale - layer.minScale) / LODSettings.numSupportedLODs;
            float current = layer.minScale + editorPreviewLOD * step * (1 + 1f / LODSettings.numSupportedLODs);

            Random.InitState(layer.seed);

            for (int i = 0; i < mHeightMap.width; i += Random.Range(1, layer.distance))
            {
                for (int j = 0; j < mHeightMap.height; j += Random.Range(1, layer.distance))
                {
                    float y = mHeightMap.values[i, j] / heightMapSettings.heightMultiplier;

                    if (y > layer.minHeight && y < layer.maxHeight)
                    {

                        float x = i * meshSettings.meshScale - meshSettings.meshWorldSize / 2;
                        float z = -j * meshSettings.meshScale + meshSettings.meshWorldSize / 2;

                        Vector2 r = Random.insideUnitCircle * layer.range;

                        Vector3 position = new Vector3(x + r.x, y * heightMapSettings.heightMultiplier, z + r.y);

                        float scale = Random.Range(layer.minScale, layer.maxScale);
                        if (scale >= current)
                        {

                            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                            go.transform.SetParent(meshRenderer.transform);
                            go.transform.localPosition = position;

                            go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                           // go.transform.localScale = Vector3.one * scale;
                        }
                    }
                }
            }
        }
    }
}
