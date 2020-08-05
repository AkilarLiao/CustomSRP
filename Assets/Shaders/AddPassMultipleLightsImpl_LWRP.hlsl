#ifndef ADD_PASS_MULTIPLE_LIGHTS_IMPL_LWRP
#define ADD_PASS_MULTIPLE_LIGHTS_IMPL_LWRP
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"

struct vertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct vertexOutput
{    
    float3 posWorld : TEXCOORD0;
    float3 normalDir : TEXCOORD1;
    float4 clipPosition : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityPerMaterial)
uniform float4 _Color;
uniform float4 _SpecColor;
uniform float _Shininess;
CBUFFER_END

vertexOutput vert(vertexInput input)
{
    vertexOutput output = (vertexOutput) 0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

    output.posWorld = mul(unity_ObjectToWorld, input.vertex).xyz;
    output.normalDir = normalize(mul(float4(input.normal, 0.0), unity_WorldToObject).xyz);

    output.clipPosition = vertexInput.positionCS;
    return output;
}

half3 ProcessLighting(in Light light, in float3 viewDirection, in float3 worldNormal)
{
    float atten = light.distanceAttenuation;
    //float atten = 1.0;
    float3 lightDirection = normalize(light.direction);
    half3 diffuseReflection = atten * light.color * saturate(dot(worldNormal,
        lightDirection));

    half3 specularReflection = atten * _SpecColor.rgb * saturate(dot(worldNormal,
        lightDirection)) * pow(saturate(dot(reflect(-lightDirection, worldNormal),
        viewDirection)), _Shininess);

    return diffuseReflection + specularReflection;
}

half4 frag(vertexOutput input) : SV_Target
{
    //Light mainLight = GetMainLight();
    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.posWorld);
    float3 worldNormal = normalize(input.normalDir);

    half3 lightFinal = ProcessLighting(GetMainLight(), viewDirection, worldNormal);
        + UNITY_LIGHTMODEL_AMBIENT.xyz;
#ifdef _ADDITIONAL_LIGHTS
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        //Light light = GetAdditionalLight(i, input.posWorld);
        lightFinal += ProcessLighting(GetAdditionalLight(i, input.posWorld),
            viewDirection, worldNormal);
    }
#endif
    return half4(lightFinal * _Color.rgb, 1.0);
}




/*
// User Defined Variables
uniform float4 _Color;
uniform float4 _SpecColor;
uniform float _Shininess;
			
// Unity Defined Variables		
uniform float4 _LightColor0;
			
// Base Input Structs
struct vertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};
struct vertexOutput
{
    float4 pos : SV_POSITION;
    float4 posWorld : TEXCOORD0;
    float3 normalDir : TEXCOORD1;
};
			
// Vertex Function
vertexOutput vert(vertexInput v)
{
    vertexOutput o;
				
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);;
				
    o.pos = UnityObjectToClipPos(v.vertex);
    return o;

}
			 
// Fragment Function
float4 frag(vertexOutput i) : COLOR
{							
    float3 normalDirection = i.normalDir;
    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
    float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
    float atten = 1.0;
				
				// Lighting
    float3 diffuseReflection = atten * _LightColor0.xyz * saturate(dot(normalDirection, lightDirection));
    float3 specularReflection = atten * _SpecColor.rgb * saturate(dot(normalDirection, lightDirection)) * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				
    float3 lightFinal = diffuseReflection + specularReflection;
#if !defined(ADD_PASS)
    lightFinal += UNITY_LIGHTMODEL_AMBIENT.xyz;
#endif		
    return float4(lightFinal * _Color.rgb, 1.0);
}*/

#endif //ADD_PASS_MULTIPLE_LIGHTS_IMPL_LWRP