#ifndef ADD_PASS_MULTIPLE_LIGHTS_IMPL_DEFAULT
#define ADD_PASS_MULTIPLE_LIGHTS_IMPL_DEFAULT

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
    float3 normalDirection = normalize(i.normalDir);
    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

    float distance = length(_WorldSpaceLightPos0.xyz - i.posWorld.xyz);
    //float distance = distance(_WorldSpaceLightPos0.xyz, i.posWorld.xyz);

    float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
    //float atten = min(distance / 500.0, 1.0);
    //float atten = min(1.0 - distance / 500000.0, 1.0);
    //float atten = 1.0 - min(distance / 15.0, 1.0);
    //float atten = 0.0;
    float atten = 1.0;
				
				// Lighting
    float3 diffuseReflection = atten * _LightColor0.xyz * saturate(dot(normalDirection, lightDirection));
    float3 specularReflection = atten * _SpecColor.rgb * saturate(dot(normalDirection, lightDirection)) * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				
    float3 lightFinal = diffuseReflection + specularReflection;
#if !defined(ADD_PASS)
    lightFinal += 0.1 * UNITY_LIGHTMODEL_AMBIENT.xyz;
#endif		
    return float4(lightFinal * _Color.rgb, 1.0);

}
#endif //ADD_PASS_MULTIPLE_LIGHTS_IMPL_DEFAULT