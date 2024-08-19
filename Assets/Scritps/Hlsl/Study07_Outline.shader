// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study07"
{
	Properties
	{
		_OutlineWidth("OutlineWidth", Float) = 1.0
		_OUtlineColor("OutlineColor", Color) = (1,1,1,1)
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

			// Render State Commands
			ZWrite On
			ZTest LEqual
			Cull[CULLBACK]

			HLSLPROGRAM
			#pragma vertex vertLine
			#pragma fragment fragOutline
			#pragma target 3.0

	

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

			float _OutlineWidth;
			float4 _OUtlineColor;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
			};

			VertexOutput vertLine(VertexInput v)
			{
				VertexOutput o;
				o.position = TransformObjectToHClip(v.vertex.xyz + v.normal * _OutlineWidth);

				return o;
			}

			half4 fragOutline(VertexOutput i) : SV_TARGET
			{
				return _OUtlineColor;
			}

			ENDHLSL
		}

	
	}
	FallBack "Diffuse"
}
