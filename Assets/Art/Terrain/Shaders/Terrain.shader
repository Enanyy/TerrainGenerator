Shader "Custom/Terrain" {
	Properties {
	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		// texture arrays are not available everywhere,
		// only compile shader on platforms where they are
		#pragma require 2darray

		const static int MAX_LAYER_COUNT = 8;
		const static float EPSILON = 1E-4;

		int _LayerCount;
		float3 _LayerColors[MAX_LAYER_COUNT];
		float _LayerStartHeights[MAX_LAYER_COUNT];
		float _LayerBlends[MAX_LAYER_COUNT];
		float _LayerColorStrengths[MAX_LAYER_COUNT];
		float _LayerTextureScales[MAX_LAYER_COUNT];

		float _MinHeight;
		float _MaxHeight;


		UNITY_DECLARE_TEX2DARRAY(_LayerTextures);

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		float InverseLerp(float a, float b, float value) 
		{
			return saturate((value-a)/(b-a));
		}

		float3 Triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) 
		{
			float3 scaledWorldPos = worldPos / scale;
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(_LayerTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(_LayerTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(_LayerTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
			return xProjection + yProjection + zProjection;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float heightPercent = InverseLerp(_MinHeight,_MaxHeight, IN.worldPos.y);
			float3 blendAxes = abs(IN.worldNormal);
			//使blendAxes.x + blendAxes.y + blendAxes.z = 1
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			for (int i = 0; i < _LayerCount; i ++) 
			{
				float drawStrength = InverseLerp(-_LayerBlends[i]/2 - EPSILON, _LayerBlends[i]/2, heightPercent - _LayerStartHeights[i]);

				float3 baseColor = _LayerColors[i] * _LayerColorStrengths[i];
				float3 textureColor = Triplanar(IN.worldPos, _LayerTextureScales[i], blendAxes, i) * (1-_LayerColorStrengths[i]);

				o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor+textureColor) * drawStrength;
			}

		
		}


		ENDCG
	}
	FallBack "Diffuse"
}
