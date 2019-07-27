Shader "Custom/line" {
	Properties 
    {
		
	}
	SubShader 
    {
        Tags 
        {
            "QUEUE" = "Geometry" 
            "RenderType" = "Opaque"
        }

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers xbox360 flash	
            #pragma multi_compile _ ENABLE_PARABOLOID

            struct appdata
            {
                half4 vertex : POSITION;
                half4 color : COLOR;
            };

            struct v2f
            {
                half4 pos		: SV_POSITION;
                half4 color     : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;								
                o.color = v.color;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {			
                half4 col = i.color; // *half4((half3)1.0, tex2D(_DiffuseAlpha,  i.uv).r);
                return col;
            }
            ENDCG
        }
    }
}
