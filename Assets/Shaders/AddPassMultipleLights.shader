// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AddPassMultipleLights"
{
	Properties
	{
		_Color( "Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		_SpecColor( "Specular Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		_Shininess( "Shininess", float ) = 10
	}	
	SubShader
	{
		LOD 500
		Pass
		{
			Name "ForwardLit"
			Tags{"LightMode" = "LightweightForward"}
			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex vert
			#pragma fragment frag

			// Lightweight Pipeline keywords
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			//#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			//#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			//#pragma multi_compile _ _SHADOWS_SOFT
			//#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#define _ADDITIONAL_LIGHTS


			#include "AddPassMultipleLightsImpl_LWRP.hlsl"
			ENDHLSL
		}
	}
	SubShader
	{
		LOD 100
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			
			// Pragmas
			#pragma vertex vert
			#pragma fragment frag
			#include "AddPassMultipleLightsImpl_Default.cginc"		
			ENDCG	
		}
		Pass
		{
			Tags { "LightMode" = "ForwardAdd" }
			Blend SrcColor One
			CGPROGRAM
			
			//#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#define _ADDITIONAL_LIGHTS
			// Pragmas
			#pragma vertex vert
			#pragma fragment frag

			#define ADD_PASS
			#include "AddPassMultipleLightsImpl_Default.cginc"
			ENDCG	
		}
	}
}
