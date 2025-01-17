Shader "Study/Study01"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader
		{
			Pass{
				Tags { "RenderType" = "Opaque" }
				LOD 200

				CGPROGRAM
				// Physically based Standard lighting model, and enable shadows on all light types
				//      (표면 쉐이더) (이름) (라이트모델) (추가 옵션)
				//#pragma surface surf Standard fullforwardshadows

				#pragma vertex vert
				#pragma fragment frag

				// #pragma vertex <name>
				// #pragma fragment <name>
				// #pragma geometry <name>
				// #pragma hull <name>
				// #pragma domain <name>

				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				sampler2D _MainTex;

				half _Glossiness;
				half _Metallic;
				fixed4 _Color;

				struct v2f
				{
					float2 uv : TEXCOORD;
					float4 vertex : SV_POSITION;
				};

				v2f vert(float4 vertex :POSITION, float2 uv : TEXCOORD)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(vertex);
					o.uv = uv;

					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET
				{
					return fixed4(i.uv,0,0);
				}

				// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
				// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
				// #pragma instancing_options assumeuniformscaling
				//UNITY_INSTANCING_BUFFER_START(Props)
				//// put more per-instance properties here
				//UNITY_INSTANCING_BUFFER_END(Props)

				ENDCG
				}
		}
			FallBack "Diffuse"
}
