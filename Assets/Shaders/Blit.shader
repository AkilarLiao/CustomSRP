Shader "Hidden/FinalBlit"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}
        LOD 100

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

			CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
			#include "UnityCG.cginc"

            struct VertexInput
            {
                float4 position	: POSITION;
                float2 uv		: TEXCOORD0;
            };

            struct VertexOutput
            {
                half4 clipPosition	: SV_POSITION;
                half2 uv			: TEXCOORD0;
            };

			uniform sampler2D _BlitTex;
			//uniform sampler2D _CameraColorTexture;

			VertexOutput VertexProgram(VertexInput input)
            {
				VertexOutput output;
                output.clipPosition = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                return output;
            }

            fixed4 FragmentProgram(VertexOutput input) : SV_Target
            {
				//return fixed4(tex2D(_CameraColorTexture, input.uv).rgb, 1.0);
				return fixed4(tex2D(_BlitTex, input.uv).rgb, 1.0);
				//return fixed4(1.0, 0.0, 0.0, 1.0);
            }
			ENDCG
        }
    }
}
