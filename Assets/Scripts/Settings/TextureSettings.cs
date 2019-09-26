using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TextureSettings : UpdatableData {

	const int textureSize = 512;
	const TextureFormat textureFormat = TextureFormat.RGB565;

	public TextureLayer[] layers;

	float savedMinHeight;
	float savedMaxHeight;

    private Color[] tints;
    private float[] startHeights;
    private float[] blendStrengths;
    private float[] tintStrengths;
    private float[] textureScales;
    private Texture2DArray textures;

    public void ApplyToMaterial(Material material)
    {
        if (tints == null || tints.Length != layers.Length) tints= new Color[layers.Length];
        if (startHeights == null || startHeights.Length != layers.Length) startHeights = new float[layers.Length];
        if (blendStrengths == null || blendStrengths.Length != layers.Length) blendStrengths = new float[layers.Length];
        if (tintStrengths == null || tintStrengths.Length != layers.Length) tintStrengths = new float[layers.Length];
        if (textureScales == null || textureScales.Length != layers.Length) textureScales = new float[layers.Length];
        if (textures == null || textures.depth != layers.Length)textures = new Texture2DArray(textureSize, textureSize, layers.Length, textureFormat, true);

        for (int i = 0; i < layers.Length; i++)
        {
            tints[i] = layers[i].tint;
            startHeights[i] = layers[i].startHeight;
            blendStrengths[i] = layers[i].blendStrength;
            tintStrengths[i] = layers[i].tintStrength;
            textureScales[i] = layers[i].textureScale;
            textures.SetPixels(layers[i].texture.GetPixels(), i);
        }
        textures.Apply();

         material.SetInt ("_LayerCount", layers.Length);
         material.SetColorArray ("_LayerColors", tints);
         material.SetFloatArray ("_LayerStartHeights", startHeights);
         material.SetFloatArray ("_LayerBlends", blendStrengths);
         material.SetFloatArray ("_LayerColorStrengths", tintStrengths);
         material.SetFloatArray ("_LayerTextureScales", textureScales);
         material.SetTexture ("_LayerTextures", textures);

		UpdateMeshHeights (material, savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
		savedMinHeight = minHeight;
		savedMaxHeight = maxHeight;

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
