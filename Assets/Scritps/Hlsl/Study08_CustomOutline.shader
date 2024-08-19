// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study08_CustomOutline"
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
			Name "CustomOutline"
			HLSLPROGRAM
			#pragma target 4.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#pragma vertex PassVertex
			#pragma fragment PassFragment

			CBUFFER_START(UnityPerMaterail)
			Texture2D _MainTex;
			SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			CBUFFER_END

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f PassVertex(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(vertex);
				o.uv = uv;

				return o;
			}

			half4 PassFragment(v2f i) :SV_TARGET
			{
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				return color;
			}

			ENDHLSL
		}

		 Pass
		{
			Name "CustomOutline"
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
