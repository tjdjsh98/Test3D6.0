// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Study/Study03"
{
	Properties
	{
		
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
				#include "UnityCG.cginc"

				// #pragma vertex <name>
				// #pragma fragment <name>
				// #pragma geometry <name>
				// #pragma hull <name>
				// #pragma domain <name>

				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				struct v2f
				{
					half3 worldRefl : TEXCOORD0;
					float4 pos : SV_POSITION;
				};

				v2f vert(float4 vertex:POSITION, float3 normal :NORMAL)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(vertex);	

					float3 worldPos = mul(unity_ObjectToWorld, vertex).xyz;

					float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)); 
				
					float3 worldNormal = UnityObjectToWorldNormal(normal);

					o.worldRefl = reflect(-worldViewDir, worldNormal);
					
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET
				{
					half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);

					half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

					fixed4 c = 0;
					c.rgb = skyColor;

					return c;
					
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
