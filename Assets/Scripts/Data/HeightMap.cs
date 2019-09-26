using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HeightMap
{
    public  Vector2 sampleCenter { get; private set; }
	public  float[,] values { get; private set; }
	public  float minValue { get; private set; }
	public  float maxValue { get; private set; }

    public readonly int width;
    public readonly int height;

    public HeightMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        values = new float[width,height];
    }

    public void GenerateNoise(NoiseSettings settings, Vector2 sampleCenter)
    {
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                values[x, y] = noiseHeight;

                if (settings.normalizeMode == NoiseSettings.NormalizeMode.Global)
                {
                    float normalizedHeight = (values[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    values[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (settings.normalizeMode == NoiseSettings.NormalizeMode.Local)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    values[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, values[x, y]);
                }
            }
        }
    }

    public void GenerateHeightMap(HeightMapSettings settings, Vector2 sampleCenter)
    {
        this.sampleCenter = sampleCenter;

        GenerateNoise(settings.noiseSettings,sampleCenter);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        minValue = float.MaxValue;
        maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] = heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }
    }

    public void GenerateFalloffMap()
    {      
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = i / (float)width * 2 - 1;
                float y = j / (float)height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                values[i, j] = FalloffEvaluate(value);
            }
        }
    }

    static float FalloffEvaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    public MeshData GenerateMeshData(MeshSettings meshSettings, int lod)
    {
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (lod == 0) ? 1 : lod * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine, meshSettings.useFlatShading);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {

                float heightY = values[x, y];

                Vector3 vertice = new Vector3((topLeftX + x) * meshSettings.meshScale, heightY,
                    (topLeftZ - y) * meshSettings.meshScale);
                Vector2 uv = new Vector2(x / (float)width, y / (float)height);

                meshData.AddVertice(vertexIndex, vertice, uv);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        meshData.ProcessMesh();

        return meshData;
    }

    public Texture2D GenerateTexture()
    {
        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(minValue, maxValue, values[x, y]));
            }
        }
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }
}

