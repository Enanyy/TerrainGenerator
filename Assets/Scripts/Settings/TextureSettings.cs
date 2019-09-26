using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TextureSettings : UpdatableData {

	const int textureSize = 512;
	const TextureFormat textureFormat = TextureFormat.RGB565;

	public TextureLayer[] layers;

	private float mMinHeight;
	private float mMaxHeight;

    private Color[] mLayerColors;
    private float[] mLayerSrartHeights;
    private float[] mLayerBlendStrengths;
    private float[] mLayerColorStrengths;
    private float[] mLayerTextureScales;
    private Texture2DArray mLayerTextures;

    public void ApplyToMaterial(Material material)
    {
        if (mLayerColors == null || mLayerColors.Length != layers.Length) mLayerColors = new Color[layers.Length];
        if (mLayerSrartHeights == null || mLayerSrartHeights.Length != layers.Length) mLayerSrartHeights = new float[layers.Length];
        if (mLayerBlendStrengths == null || mLayerBlendStrengths.Length != layers.Length) mLayerBlendStrengths = new float[layers.Length];
        if (mLayerColorStrengths == null || mLayerColorStrengths.Length != layers.Length) mLayerColorStrengths = new float[layers.Length];
        if (mLayerTextureScales == null || mLayerTextureScales.Length != layers.Length) mLayerTextureScales = new float[layers.Length];
        if (mLayerTextures == null || mLayerTextures.depth != layers.Length) mLayerTextures = new Texture2DArray(textureSize, textureSize, layers.Length, textureFormat, true);

        for (int i = 0; i < layers.Length; i++)
        {
            mLayerColors[i] = layers[i].tint;
            mLayerSrartHeights[i] = layers[i].startHeight;
            mLayerBlendStrengths[i] = layers[i].blendStrength;
            mLayerColorStrengths[i] = layers[i].tintStrength;
            mLayerTextureScales[i] = layers[i].textureScale;
            mLayerTextures.SetPixels(layers[i].texture.GetPixels(), i);
        }
        mLayerTextures.Apply();

        material.SetInt("_LayerCount", layers.Length);
        material.SetColorArray("_LayerColors", mLayerColors);
        material.SetFloatArray("_LayerStartHeights", mLayerSrartHeights);
        material.SetFloatArray("_LayerBlends", mLayerBlendStrengths);
        material.SetFloatArray("_LayerColorStrengths", mLayerColorStrengths);
        material.SetFloatArray("_LayerTextureScales", mLayerTextureScales);
        material.SetTexture("_LayerTextures", mLayerTextures);

        UpdateMeshHeights(material, mMinHeight, mMaxHeight);
    }

	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
		mMinHeight = minHeight;
		mMaxHeight = maxHeight;

        material.SetFloat ("_MinHeight", minHeight);
        material.SetFloat ("_MaxHeight", maxHeight);
	}

	[System.Serializable]
	public class TextureLayer {
		public Texture2D texture;
		public Color tint;
		[Range(0,1)]
		public float tintStrength;
		[Range(0,1)]
		public float startHeight;
		[Range(0,1)]
		public float blendStrength;
		public float textureScale;
	}
		
	 
}
