// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Similar to regular FX/Glass/Stained BumpDistort shader
// from standard Effects package, just without grab pass,
// and samples a texture with a different name.

Shader "FX/Glass/Stained BumpDistort (no grab)" {
Properties {
	_TintAmt ("Tint Amount", Range(0,1)) = 0.1
	_Color ("Tint Color (RGB)", Color) = (0, 0, 0, 0)
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }

	SubShader {

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord: TEXCOORD0;
};

struct v2f {
	float4 vertex : POSITION;
	float4 uvgrab : TEXCOORD0;
};

half _TintAmt;
float4 _Color;
float _YFlipOverride;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
    float scale;
#if UNITY_UV_STARTS_AT_TOP
	scale = -1;
#else
	scale = 1;
#endif
	o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
	o.uvgrab.zw = o.vertex.zw;
	return o;
}

sampler2D _GrabBlurTexture;
float4 _GrabBlurTexture_TexelSize;
sampler2D _BumpMap;

half4 frag (v2f i) : SV_Target
{
	i.uvgrab.xy = i.uvgrab.xy;
	half4 col = tex2Dproj (_GrabBlurTexture, UNITY_PROJ_COORD(i.uvgrab));
	col = saturate(lerp (col, _Color, _TintAmt));
	return col ;
}
ENDCG
		}
	}

}

}
