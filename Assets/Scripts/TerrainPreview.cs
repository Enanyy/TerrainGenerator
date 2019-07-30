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
        int childCount = meshRenderer.transform.childCount;
        

        if (generateTree == false)
        {
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = meshRenderer.transform.GetChild(i);
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            return;
        }

        int index = 0;

        for (int k = 0; k < treeSettings.trees.Length; k++)
        {
            var layer = treeSettings.trees[k];

            float step = (layer.maxScale - layer.minScale) / LODSettings.numSupportedLODs;
            float current = layer.minScale + editorPreviewLOD * step * (1 + 1f / LODSettings.numSupportedLODs);

            System.Random random = new System.Random(layer.seed);

            for (int i = 0; i < mHeightMap.width; i += random.Next(1, layer.distance))
            {
                for (int j = 0; j < mHeightMap.height; j += random.Next(1, layer.distance))
                {
                    float y = mHeightMap.values[i, j] / heightMapSettings.heightMultiplier;

                    if (y > layer.minHeight && y < layer.maxHeight)
                    {

                        float x = i * meshSettings.meshScale - meshSettings.meshWorldSize / 2;
                        float z = -j * meshSettings.meshScale + meshSettings.meshWorldSize / 2;

                        float rx = random.Next(0, 100) / 100f;
                        float ry = random.Next(0, 100) / 100f;

                        Vector3 position = new Vector3(x + rx, y * heightMapSettings.heightMultiplier, z + ry);

                        float scale = random.Next((int)(layer.minScale * 100), (int)(layer.maxScale * 100)) / 100f;
                        if (scale >= current)
                        {

                            GameObject go = null;
                            if (index < childCount)
                            {
                                go = meshRenderer.transform.GetChild(index).gameObject;
                            }
                            else
                            {
                               go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            }


                            go.transform.SetParent(meshRenderer.transform);
                            go.transform.localPosition = position;

                            go.transform.rotation = Quaternion.Euler(0, random.Next(0, 360), 0);

                            index++;
                        }
                    }
                }
            }
        }

        for (int i = childCount - 1; i  >= index; i --)
        {
            var child = meshRenderer.transform.GetChild(i);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
