Shader "CustomURP/PlaneReflectionTexture"
{
    Properties
    {
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
            };

            struct VertexOutput
            {
				float4 positionCS : SV_POSITION;
				float3 localNormal : TEXCOORD0;
				float4 screenPosition : TEXCOORD1;
            };

			TEXTURE2D(_PlanarReflectionTexture); SAMPLER(sampler_PlanarReflectionTexture);
			
			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			CBUFFER_END

            VertexOutput VertexProgram(VertexInput input)
            {
				VertexOutput output = (VertexOutput)0;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.localNormal = input.normalOS;
				output.screenPosition = ComputeScreenPos(output.positionCS);
                return output;
            }

			half4 FragmentProgram(VertexOutput input) : SV_Target
			{
				float3 localNormal = normalize(input.localNormal);
				float dotValue = dot(localNormal, float3(0.0, 1.0, 0.0));
				//float ratio = smoothstep(0.5, 1.0, dotValue);
				float ratio = step(0.5, dotValue);
				half4 col = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture,
					input.screenPosition.xy / input.screenPosition.w);
				col.rgb = lerp(_Color.rgb, col.rgb, ratio);
				return col;
            }
			ENDHLSL
        }
    }
}
