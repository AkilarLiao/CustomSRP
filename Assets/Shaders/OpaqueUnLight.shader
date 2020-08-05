Shader "Unlit/OpaqueUnLight"
{
    Properties
    {
		_BaseColor("Color", Color) = (1, 1, 1, 1)
		_OutlineColor("OutlineColor", Color) = (1, 1, 1, 1)
		_OutlineWidth("Outline width", Range(0.0, 1.0)) = .005
    }
    SubShader
    {
		Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

			fixed3 _BaseColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return fixed4(_BaseColor, 1.0);
            }
            ENDCG
        }
		Pass
		{
			Name "Ouline"
			Tags{"LightMode" = "OutLine"}
			Cull front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			fixed3 _OutlineColor;
			float _OutlineWidth;

			v2f vert(appdata v)
			{
				v2f o;
				float3 normal = normalize(v.normal);
				v.vertex.xyz += v.normal * _OutlineWidth;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(_OutlineColor, 1.0);
			}
			ENDCG
		}
    }
}
