// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study08"
{
	Properties
	{
		_Value("Value",Float) = 0
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Transparent"
		}
		LOD 200

		Pass
		{
			Name "CustomOutline"
			HLSLPROGRAM
			#pragma target 2.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			#pragma vertex PassVertex
			#pragma fragment PassFragment

			#include "Noise.hlsl"

			CBUFFER_START(UnityPerMaterail)
			Texture2D _MainTex;
			SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			float _Value;
			CBUFFER_END

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 positionSS : POSITION1;
				float2 uv : TEXCOORD0;
			};

			v2f PassVertex(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
			{
				

				VertexPositionInputs  vertexInput = GetVertexPositionInputs(vertex.xyz);

				v2f o;

				o.vertex = vertexInput.positionCS;
				o.positionSS = vertexInput.positionVS;
				
				float positionWS = mul(UNITY_MATRIX_M, vertex);
				
				
				
				return o;
			}

			half4 PassFragment(v2f i) :SV_TARGET
			{
				float2 uv = i.vertex.xy / _ScaledScreenParams.xy;
				float4 depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, uv),_ZBufferParams);
				
				float depthMapDepth = depth.x;
				float myDepth = i.vertex.w;
				float distance = depthMapDepth - myDepth;

			/*	if (_Value > distance)
					return float4(0, 0, 0, 0);
				else
					return float4(1,1,1,1);*/


				return float4(distance,distance,distance,0);
			}

			ENDHLSL
		}
		 Pass
		{
			Name "DepthNormalsOnly"
			Tags
			{
				"LightMode" = "DepthNormalsOnly"
			}

				// -------------------------------------
				// Render State Commands
				ZWrite On
				Cull[_Cull]

				HLSLPROGRAM
				#pragma target 2.0

				// -------------------------------------
				// Shader Stages
				#pragma vertex DepthNormalsVertex
				#pragma fragment DepthNormalsFragment

				// -------------------------------------
				// Material Keywords
				#pragma shader_feature_local _NORMALMAP
				#pragma shader_feature_local _PARALLAXMAP
				#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
				#pragma shader_feature_local _ALPHATEST_ON
				#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

				// -------------------------------------
				// Universal Pipeline keywords
				#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT // forward-only variant
				#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

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
				#include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
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
			 ColorMask R
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
			 #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
			 ENDHLSL
		 }
	}

	FallBack "Diffuse"
}
