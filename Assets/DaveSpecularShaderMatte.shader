// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SpecularShaderMatte" {
	Properties {
		_Color ("Albedo (RGB)", Color) = (0,0,0,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_HighlightThreshold ("Highlight Threshold", Range(0,1)) = 0
		_Bump ("Normal Map", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1.75)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Fade"
			   "Queue" = "Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf DavePhong

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed4 _Color;
		sampler2D _Bump;

		struct Input {
			float2 uv_BumpMap;
			float3 worldPos;
//			float3 vertexPos;
//			float3x3 rot;
		};

		struct SurfaceOutputDave {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
			float3 worldPos;
			float3 vertexPos;
		};

		half _Glossiness;
		fixed4 _Specular;
		half _HighlightThreshold;

//		void vert(inout appdata_full v, out Input o){
//			UNITY_INITIALIZE_OUTPUT(Input, o);
//			float3 dir = mul((float3x3)unity_WorldToObject, float3(0.0,1.0,0.0));
////			float3 t = normalize(dir - v.vertex.xyz);
//			TANGENT_SPACE_ROTATION;
//			float3 t2 = mul(rotation, dir) - mul(rotation, v.vertex.xyz);
////			o.vertexPos = normalize(t2);
//			o.vertexPos = v.vertex.xyz;
//		}

		void surf (Input IN, inout SurfaceOutputDave o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = _MainTex.rgb;
			o.Albedo = _Color.rgb;
			//o.Specular = _Specular
			// Metallic and smoothness come from slider variables
			o.Specular = _Glossiness;
			o.Gloss = 1;
			o.Alpha = _Color.a;
			o.Normal = UnpackNormal (tex2D (_Bump, IN.uv_BumpMap));
//			o.vertexPos = IN.vertexPos;
			o.worldPos = IN.worldPos;
		}

		float4 _initCameraPos;
		half4 LightingDavePhong (SurfaceOutputDave s, half3 viewDir, half3 lightDir, half atten)
		{
//			half3 h = normalize(lightDir + normalize(mul((float3x3)UNITY_MATRIX_MVP,half3(0.0,1.0,0.0) - s.worldPos)));
//			half3 h = normalize(lightDir + normalize(mul((float3x3)unity_WorldToObject, half3(0.0,1.0,0.0))- s.worldPos));
//			half3 h = normalize(lightDir + normalize(float3(0.0,1.0,0.0) - s.worldPos));
			half3 h = normalize(lightDir + normalize(UnityWorldSpaceViewDir(s.worldPos)));
//			half3 h = normalize(lightDir + viewDir);
			fixed diff = max (0, dot (s.Normal, lightDir));
			
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular*512);
			
			half4 c;
			//c.rgb = s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * _Specular.rgb * spec;
			c.rgb = (s.Albedo*_LightColor0.rgb*diff + _LightColor0.rgb*spec)*atten;
			c.a = s.Alpha;
			if(sqrt(c.r*c.r+c.g*c.g+c.b*c.b) < _HighlightThreshold){
				c.rgb = 0;
			}

			return c;
		}
		ENDCG
	}
	FallBack "Specular"
}
