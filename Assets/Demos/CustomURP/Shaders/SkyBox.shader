Shader "CustomShader/Skybox"
{
	Properties
	{
		_Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
		_PassOffDepthRatio("PassOffDepthRatio", Range(0.01, 0.99)) = 0.8
		[HideInInspector]_ZTest("_ZTest", Int) = 4 // Less equal
	}
	SubShader
	{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off
		ZWrite Off
		ZTest[_ZTest]
		Pass
		{
			CGPROGRAM
			#pragma multi_compile _ _PROCESS_DISSOLVE
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#define SKYBOX_THREASHOLD_VALUE 0.9999

			samplerCUBE _Tex;
			half4 _Tex_HDR;
			half4 _Tint;
			half _Exposure;
			float _Rotation;

			//原生的深度貼圖，不用拷貝，直接傳入
			uniform sampler2D _CameraDepthAttachment;
			//在畫天空盒之前，拷貝目前畫面的結果
			uniform sampler2D _CopyBackgroundRT;			
			
			uniform fixed3 _FogColor;
			uniform float _PassOffDepthRatio;

			float3 RotateAroundYInDegrees(float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				float4 screenPosition : TEXCOORD1;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
				o.vertex = UnityObjectToClipPos(rotated);
				o.texcoord = v.vertex.xyz;
				o.screenPosition = ComputeScreenPos(o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 tex = texCUBE(_Tex, i.texcoord);
				half3 c = DecodeHDR(tex, _Tex_HDR);
				c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
				c *= _Exposure;
				half3 skyColor = c;
#if !defined (_PROCESS_DISSOLVE)
				return fixed4(skyColor, 1.0);
#endif
				half2 screenUV = i.screenPosition.xy / i.screenPosition.w;
				fixed4 backGround = tex2D(_CopyBackgroundRT, screenUV);

				fixed3 result = (1.0 - backGround.a) * skyColor + backGround.a * backGround.rgb;

				float depth = Linear01Depth(tex2D(_CameraDepthAttachment, screenUV).r);
				
				if (depth >= _PassOffDepthRatio)
				{
					float depthLength = 1.0 - _PassOffDepthRatio;
					float deltaLength = depthLength - (1.0 - depth);

					result = lerp(result, skyColor, 1.0 - (depthLength - deltaLength) / depthLength);
				}
				return fixed4(result, backGround.a);
			}
			ENDCG
		}		
	}
	Fallback Off
}