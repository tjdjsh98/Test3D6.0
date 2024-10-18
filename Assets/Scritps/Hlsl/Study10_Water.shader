// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study10"
{
	Properties
	{
		_Depth("Depth",Float) = 0
		_Strength("Strength", Float) = 0
		_ShadowWaterColor("ShadowWaterColor", Color) = (0,0,0)
		_DeepWaterColor("DeepWaterColor", Color) = (0,0,0)
		_NormalMap("Normal Map", 2D) = "Normal" {}
	
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
		ZWrite Off
		Blend One SrcAlpha
		Pass
		{
			Name "CustomOutline"
			HLSLPROGRAM
			#pragma target 2.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

			#pragma vertex PassVertex
			#pragma fragment PassFragment

			#include "../../Materials/Hlsl/Common.hlsl"

			CBUFFER_START(UnityPerMaterail)
			Texture2D _MainTex;
			SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			Texture2D _NormalMap;
			SAMPLER(sampler_NormalMap);
			float4 _NormalMap_ST;


			float _Depth;
			float _Strength;
			float4 _ShadowWaterColor;
			float4 _DeepWaterColor;

			CBUFFER_END

			////////////////
			/// Struct	////
			////////////////

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normalWS : NORMAL;

				float2 uv : TEXCOORD0;
				float3 tangentWS : TEXCOORD1;
				float3 bitangentWS : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
			};
			
			/////////////////
			///// Func  /////
			/////////////////

			v2f PassVertex(VertexInput input)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(input.vertex.xyz);
				o.normalWS = TransformObjectToWorldNormal(input.normal);
				o.tangentWS = TransformObjectToWorldDir(input.tangent);
				o.bitangentWS = cross(o.normalWS, o.tangentWS) * input.tangent.w * unity_WorldTransformParams.w;

				o.viewDir = _WorldSpaceCameraPos.xyz - TransformObjectToWorld(input.vertex.xyz);
				o.uv = input.uv;
				return o;
			}

			half4 PassFragment(v2f i) :SV_TARGET
			{
				//float3 lightDir = _MainLightPosition.xyz;          
				//float3 lightColor = _MainLightColor.rgb;
				//float3 viewDir = normalize(i.viewDir);


				float2 normalUV = i.uv * _NormalMap_ST.xy + _NormalMap_ST.zw;
				half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalUV));

				float3x3 TBN = float3x3(i.tangentWS, i.bitangentWS, i.normalWS);
				float3 worldNormal = mul(bump, TBN);
				
				float NdotL = saturate(dot(_MainLightPosition.xyz, worldNormal));

				float2 uv = i.vertex.xy / _ScaledScreenParams.xy;
				float4 depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, uv), _ZBufferParams);
				float realDepth = i.vertex.w;

				SurfaceData surfaceData;
				InitializeStandardLitSurfaceData(i.uv, surfaceData);

				realDepth += _Depth;
				depth -= realDepth;
				depth *= _Strength;

				if (depth.x < 0) depth.x = 0;
				if (depth.x > 1) depth.x = 1;

				//clamp(depth, 0, 1);
				
				

				float3 fReflection = reflect(_MainLightPosition.xyz, worldNormal);
				float fSpec_Phong = saturate(dot(fReflection, -normalize(i.viewDir)));
				fSpec_Phong = pow(fSpec_Phong, 30.0f);

				float4 color = Lerp(_ShadowWaterColor, _DeepWaterColor, depth.x);
				color = (color + color * NdotL + fSpec_Phong)/2;


				return color;

			}

			ENDHLSL
		}

		// ComplexLit 에서 가져온 코드
		// 깊이 노말 만들 때 사용
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

		// ComplexLit 에서 가져온 코드
		// 깊이맵 만들 때 사용
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
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			 #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
			 ENDHLSL
		 }
	}

	FallBack "Diffuse"
}
