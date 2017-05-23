// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "DaveShaderTest" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_specColor ("Specular Color", Color) = (1,1,1,1)
		_GlossinessThreshold ("GThreshold", Range(0,1)) = 0.5
		_Shininess ("shineness", Range(0,50)) = 10
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Pass{
			Tags { "RenderType" = "Opaque" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			sampler2D _MainTex;
			float4 _Color;
			float4 _specColor;
			float _Shininess;
			float4 _LightColor0;

			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			vertexOutput vert(vertexInput input) {
				vertexOutput output;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, input.vertex).xyz);
				float3 normalDirection = normalize(mul(input.normal, unity_WorldToObject));
				float3 vertexToLightSource = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, input.vertex).xyz;
				float3 viewDirection = normalize(vertexToLightSource);
				float atten = 1 / length(vertexToLightSource);
				float3 specularReflection = atten*_LightColor0.rgb*_specColor.rgb 
					* pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				output.col = float4(specularReflection, input.vertex.a);
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return input.col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}