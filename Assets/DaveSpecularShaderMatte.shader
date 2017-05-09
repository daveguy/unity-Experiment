// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SpecularShaderMatte" {
	Properties {
		_Color ("Albedo (RGB)", Color) = (0,0,0,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_HighlightThreshold ("Highlight Threshold", Range(0,1)) = 0
		_WhiteThreshold ("white threshold", Range(0.001, 1)) = 1
		_reverseColors("reverse colors", Range(0,1)) = 1
		_mainTex("texture", 2D) = "white" {}
		_Bump ("Normal Map", 2D) = "bump" {}
		_SpecularPower ("SpecularPower", Range(0,1.75)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Fade"
			   "Queue" = "Transparent" }
		GrabPass{"_GrabTexture"}
//		LOD 200


		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf DavePhong 
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		struct Input {
			float2 uv_BumpMap;
			float2 uv_MainTex;
			float3 worldPos;
			float4 uvGrab;
			float4 initCameraPos;
		};

		struct SurfaceOutputDave {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Specular;
			fixed3 Emission;
			fixed SpecularPower;
			fixed Alpha;
			float3 worldPos;
			fixed3 AlbedoTex;
		};

		fixed4 _Color;
		fixed4 _Specular;
		half _HighlightThreshold;
		half _WhiteThreshold;
		fixed _reverseColors;
		sampler2D _Bump;
		sampler2D _mainTex;
		half _SpecularPower;
		sampler2D _GrabTexture;
		float4 _initCameraPos;

		void surf (Input IN, inout SurfaceOutputDave o) {
			o.Albedo = _Color.rgb;
			o.Specular = _Specular.rgb;
			o.SpecularPower = _SpecularPower;
			o.Alpha = _Color.a;
			o.Normal = UnpackNormal (tex2D (_Bump, IN.uv_BumpMap));
			o.worldPos = IN.worldPos;
			float4 objPos = mul(unity_WorldToObject, IN.worldPos);
			o.uvGrab = ComputeGrabScreenPos(UnityObjectToClipPos(objPos));
//			o.AlbedoTex = tex2D(_mainTex, IN.uv_MainTex).rgb;
		}

		half4 LightingDavePhong (SurfaceOutputDave s,  half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize(lightDir + normalize(_initCameraPos.xyz - s.worldPos));
			fixed diff = max (0, dot (s.Normal, lightDir));
			
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.SpecularPower*512);

			half3 objectCol;
//			s.Albedo *= s.AlbedoTex;
			objectCol = (s.Albedo*_LightColor0.rgb*diff + _LightColor0.rgb*s.Specular*spec)*atten;

			//All of my various different testing options
			if(sqrt(objectCol.r*objectCol.r+objectCol.g*objectCol.g+objectCol.b*objectCol.b) < _HighlightThreshold){
				objectCol.rgb = 0;
			}
			if(objectCol.r > _WhiteThreshold){
				objectCol.rgb =1;
			}
			if(_reverseColors < 0.5){
				objectCol = half3(0.2,0.2,0.2) - objectCol;
			}
			half4 c;
//			half3 test = lerp(half3(0.0,0.0,0.0), objectCol, s.Alpha);
//			c.rgb = test;
			c.rgb = objectCol;
			c.a = s.Alpha;


			return c;
		}
		ENDCG
	}
	FallBack "Specular"
}
