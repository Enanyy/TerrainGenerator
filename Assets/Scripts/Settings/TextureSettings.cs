using UnityEngine;
using System.Collections;
using System.Linq;

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
    private float[] tintStrengts;
    private float[] textureScales;
    private Texture2DArray textures;

    public void ApplyToMaterial(Material material)
    {

        if (tints == null || tints.Length == 0) tints = layers.Select(x => x.tint).ToArray();
        if (startHeights == null || startHeights.Length == 0) startHeights = layers.Select(x => x.startHeight).ToArray();
        if (blendStrengths == null|| blendStrengths.Length == 0) blendStrengths = layers.Select(x => x.blendStrength).ToArray();
        if (tintStrengts == null || tintStrengts.Length == 0) tintStrengts = layers.Select(x => x.tintStrength).ToArray();
        if (textureScales == null || textureScales.Length == 0) textureScales = layers.Select(x => x.textureScale).ToArray();
        if (textures == null ) textures = GenerateTextureArray(layers.Select(x => x.texture).ToArray()); ;


        material.SetInt ("layerCount", layers.Length);
		material.SetColorArray ("baseColours", tints);
		material.SetFloatArray ("baseStartHeights", startHeights);
		material.SetFloatArray ("baseBlends", blendStrengths);
		material.SetFloatArray ("baseColourStrength", tintStrengts);
		material.SetFloatArray ("baseTextureScales", textureScales);
		material.SetTexture ("baseTextures", textures);

		UpdateMeshHeights (material, savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
		savedMinHeight = minHeight;
		savedMaxHeight = maxHeight;

		material.SetFloat ("minHeight", minHeight);
		material.SetFloat ("maxHeight", maxHeight);
	}

	Texture2DArray GenerateTextureArray(Texture2D[] textures) {
		Texture2DArray textureArray = new Texture2DArray (textureSize, textureSize, textures.Length, textureFormat, true);
		for (int i = 0; i < textures.Length; i++) {
			textureArray.SetPixels (textures [i].GetPixels (), i);
		}
		textureArray.Apply ();
		return textureArray;
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
