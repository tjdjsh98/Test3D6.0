// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study09"
{
	Properties
	{
		_Value("Value",Float) = 0
		_NoiseScale("NoiseScale", Float) =1
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry"
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
			float _NoiseScale;
			CBUFFER_END

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 positionSS : POSITION1;
				float2 uv : TEXCOORD0;
			};

			inline float2 unity_voronoi_noise_randomVector(float2 UV, float offset)
			{
				float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
				UV = frac(sin(mul(UV, m)) * 46839.32);
				return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
			}

			void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
			{
				float2 g = floor(UV * CellDensity);
				float2 f = frac(UV * CellDensity);
				float t = 8.0;
				float3 res = float3(t, 0.0, 0.0);

				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						float2 lattice = float2(x, y);
						float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
						float d = distance(lattice + offset, f);
						if (d < res.x)
						{
							res = float3(d, offset.x, offset.y);
							Out = res.x;
							Cells = res.y;
						}
					}
				}
			}

			v2f PassVertex(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
			{
				
				VertexPositionInputs  vertexInput = GetVertexPositionInputs(vertex.xyz);

				v2f o;

				o.vertex = vertexInput.positionCS;
				o.positionSS = vertexInput.positionVS;
				o.uv = uv;
				float positionWS = mul(UNITY_MATRIX_M, vertex);
				
				
				
				return o;
			}

			half4 PassFragment(v2f i) :SV_TARGET
			{
				float noise = ClassicNoise(i.uv * _NoiseScale);
				float _out = 0;
				float _cells = 0;
				Unity_Voronoi_float(i.uv, 12, _NoiseScale, _out, _cells);
			/*	if (noise < _Value)
					return float(0);
				else
					return float(1);*/

				if (_out < 1)
					_out = 0;

				return float4(_out, _out, _out,0);
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
