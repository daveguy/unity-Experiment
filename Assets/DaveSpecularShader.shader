Shader "Custom/DaveSpecularShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_HighlightThreshold ("Highlight Threshold", Range(0,1)) = 0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Bump ("height map", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Fade"
			   "Queue" = "Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Bump;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Bump;
			float3 viewDir;
			float3 worldNormal; INTERNAL_DATA
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _HighlightThreshold;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			float3 normal = UnpackNormal (tex2D (_Bump, IN.uv_Bump));
			o.Normal = normal;
			if(saturate(dot(normalize(IN.viewDir), normal)) < _HighlightThreshold){
				o.Smoothness = 1;
			}
		}
		ENDCG
	}
	FallBack "Specular"
}
