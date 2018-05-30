Shader "Core/Standard" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecFalloff("Specular Falloff", Range(0.05, 1.0)) = 0.5
		_SpecIntensity("Specular Intensity", Range(0.0, 2.0)) = 0.5
		_BumpMapIntensity("Normal Intensity", Range(-1.0, 1.0)) = 1.0
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_OcclusionIntensity("Occlusion Intensity", Range(0.0, 1.0)) = 1.0
		_Occlusion ("Occlusion", 2D) = "white" {}

		_Attenuation ("Attenuation", Range(0.0, 1.0)) = 0.75
		_RimFalloff("Rimlight Falloff", Range (0.0, 20.0)) = 10.0
		_RimIntensity("Rimlight Intensity", Range(0.0, 1.0)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Test fullforwardshadows

		struct Input
		{
			float2 uv_MainTex : TEXCOORD0;
			float2 uv_BumpMap;
			float3 lightDir;
			float3 viewDir;
			float3 worldPos;
		};

		half _Attenuation;
		half _SpecFalloff;
		half _SpecIntensity;
		float _BumpMapIntensity;

		half4 LightingTest(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			float normal = _BumpMapIntensity;
			s.Normal *= normal;
			half3 shade = dot(s.Normal, lightDir);
			half3 wrappedShade = shade * _Attenuation + 0.5;

			half3 highlights = normalize(lightDir + viewDir);

			float normalHighlights = max(0, dot (s.Normal, highlights));
			float spec = pow(normalHighlights, max(0.1, _SpecFalloff * 60) * _SpecFalloff * 30) * _SpecIntensity;

			spec *= _LightColor0.rgb;

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * wrappedShade + spec) * atten;
			c.a = s.Alpha;

			return c;
		}

		sampler2D _MainTex;
		float4 _Color;
		sampler2D _BumpMap;
		float _OcclusionIntensity;
		sampler2D _Occlusion;
		float _RimFalloff;
		float _RimIntensity;

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 color = tex2D(_MainTex, IN.uv_MainTex);
			half4 occlusion = tex2D(_Occlusion, IN.uv_MainTex);

			o.Albedo = color.rgb * _Color;
			o.Albedo *= pow(occlusion.rgb, _OcclusionIntensity);
			o.Alpha = color.a;
			o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = (_LightColor0.rgb * pow (rim, _RimFalloff) * _RimIntensity);
			//Occlude
			o.Emission *= pow(occlusion, _OcclusionIntensity * 5);
			//Albedo influence
			o.Emission *= color.rgb * _Color * max(0, _RimIntensity);

			o.Normal *= _BumpMapIntensity;
		}

		ENDCG
	}
	FallBack "Specular"
}
