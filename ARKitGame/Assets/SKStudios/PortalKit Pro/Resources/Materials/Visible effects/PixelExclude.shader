Shader "Custom/PixelExclude" {
	Properties{
		_MainTex("Color (RGB)", 2D) = "white" {}
		_Color("Tint", Color) = (1.0, 0.6, 0.6, 1.0)
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_DetailMask("Detail", 2D) = "white" {}
		[Gamma]  _Metallic("Metallic", Range(0.000000,1.000000)) = 0.000000
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		//_Cube("Cubemap", CUBE) = "" {}
		//_ClipVector("Clip Vector", Vector) = (0, 0, 0)
		//_ClipPosition("Clip Position", Vector) = (0, 0, 0)
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		Cull Back
		CGPROGRAM
		#pragma surface surf Lambert finalcolor:mycolor
		struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float2 uv_Detail;
		float2 uv_MetallicGlossMap;
		float3 worldPos;
		float3 worldRefl;
		INTERNAL_DATA
	};
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _Detail;
	sampler2D _MetallicGlossMap;
	//samplerCUBE _Cube;

	float3 _ClipPosition;
	float3 _ClipVector;
	float _ClipOverride;
	fixed4 _Color;

	void mycolor(Input IN, SurfaceOutput o, inout fixed4 color)
	{
		color *= _Color;
	}
	void surf(Input IN, inout SurfaceOutput o) {
		if(!_ClipOverride)
			clip(dot(_ClipVector, IN.worldPos - _ClipPosition));
		o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		o.Albedo *= tex2D(_Detail, IN.uv_Detail).rgb * 2;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		float4 metallicMap = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap);
		o.Gloss = metallicMap.r;
		o.Specular = metallicMap.g;
		//o.Emission = texCUBE(_Cube, IN.worldRefl).rgb;
		o.Alpha = 1;
	}
	ENDCG
	}
		Fallback "Diffuse"
}
/*
Shader "Custom/PixelExclude" {
Properties{
_MainTex("Texture", 2D) = "white" {}
_Color("Tint", Color) = (1.0, 0.6, 0.6, 1.0)
//_ClipVector("Clip Vector", Vector) = (0, 0, 0)
//_ClipPosition("Clip Position", Vector) = (0, 0, 0)
}
SubShader{
Tags{ "RenderType" = "Opaque" }
CGPROGRAM
#pragma surface surf Lambert finalcolor:mycolor
struct Input {
float2 uv_MainTex;
};
fixed4 _Color;
void mycolor(Input IN, SurfaceOutput o, inout fixed4 color)
{
color *= _Color;
}
sampler2D _MainTex;
void surf(Input IN, inout SurfaceOutput o) {
clip(dot(_ClipVector, IN.worldPos - _ClipPosition));

o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
}
ENDCG
}
Fallback "Diffuse"
}*/