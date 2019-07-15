using UnityEngine;
using System.Collections;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData {

	const int textureSize = 512;
	const TextureFormat textureFormat = TextureFormat.RGB565;

	public Layer[] layers;

	float savedMinHeight;
	float savedMaxHeight;

    private Color[] baseColours;
    private float[] baseStartHeights;
    private float[] baseBlends;
    private float[] baseColourStrength;
    private float[] baseTextureScales;
    private Texture2DArray texturesArray;


    public void ApplyToMaterial(Material material)
    {
        if (baseColours == null) baseColours = layers.Select(x => x.tint).ToArray();
        if (baseStartHeights == null) baseStartHeights = layers.Select(x => x.startHeight).ToArray();
        if (baseBlends == null) baseBlends = layers.Select(x => x.blendStrength).ToArray();
        if (baseColourStrength == null) baseColourStrength = layers.Select(x => x.tintStrength).ToArray();
        if (baseTextureScales == null) baseTextureScales = layers.Select(x => x.textureScale).ToArray();
        if (texturesArray == null) texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());


        material.SetInt ("layerCount", layers.Length);
		material.SetColorArray ("baseColours", baseColours);
		material.SetFloatArray ("baseStartHeights", baseStartHeights);
		material.SetFloatArray ("baseBlends", baseBlends);
		material.SetFloatArray ("baseColourStrength", baseColourStrength);
		material.SetFloatArray ("baseTextureScales", baseTextureScales);	
		material.SetTexture ("baseTextures", texturesArray);

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
	public class Layer {
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
