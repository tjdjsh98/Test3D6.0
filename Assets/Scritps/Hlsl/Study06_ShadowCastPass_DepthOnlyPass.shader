// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study06"
{
	Properties
	{

	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry+0"
		}
		LOD 200

		 Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			// -------------------------------------
			// Render State Commands
			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[CULL BACK]

			HLSLPROGRAM
			#pragma target 2.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


			// -------------------------------------
			// Shader Stages
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

			// -------------------------------------
			// Universal Pipeline keywords

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE

			// This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
			#include "HLSLSupport.cginc"

			CBUFFER_START(UnityPerMaterial)
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};
			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
			};

			VertexOutput ShadowPassVertex(VertexInput v)
			{
				VertexOutput o;
				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 normalWS =	TransformObjectToWorldNormal(v.normal.xyz);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));

				o.vertex = positionCS;

				return o;
			}

			half4 ShadowPassFragment(VertexOutput i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}


		 Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
			}

			// -------------------------------------
			// Render State Commands
			ZWrite On
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM
			#pragma target 2.0

			// -------------------------------------
			// Shader Stages
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

			// -------------------------------------
			// Includes
			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
			CBUFFER_END

			struct VertexInput
			{
			float4 vertex : POSITION;
			};
			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
			};

			VertexOutput DepthOnlyVertex(VertexInput v)
			{
				VertexOutput o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);

				return o;
			}

			half4 DepthOnlyFragment(VertexOutput IN) : SV_TARGET
			{
				return 0;
			}

			ENDHLSL
		}
	}
	FallBack "Diffuse"
}
