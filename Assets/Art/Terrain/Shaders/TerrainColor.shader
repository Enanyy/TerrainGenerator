Shader "Custom/TerrainColor" {
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

		float _MinHeight;
		float _MaxHeight;

		struct Input {
			float3 worldPos;
		};

		float InverseLerp(float a, float b, float value)
		{
			return saturate((value-a)/(b-a));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float heightPercent = InverseLerp(_MinHeight,_MaxHeight, IN.worldPos.y);

			for (int i = 0; i < _LayerCount; i ++)
			{
				//float drawStrength = saturate(sign(heightPercent - _LayerStartHeights[i]));
				
				float drawStrength = InverseLerp(-_LayerBlends[i]/2 - EPSILON, _LayerBlends[i]/2, heightPercent - _LayerStartHeights[i]);

				o.Albedo = o.Albedo * (1 - drawStrength) + _LayerColors[i] * drawStrength;
			}
		}


		ENDCG
	}
	FallBack "Diffuse"
}