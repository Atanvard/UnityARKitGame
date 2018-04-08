// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlitWithInversion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	    _Offset("Offset", Float) = -1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		ZTest LEqual
		ZWrite On
		Offset 0,[_Offset]
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
		{
			float4 vertex : POSITION;
			float4 uv:TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 screenPos:TEXCOORD1;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.screenPos = ComputeScreenPos(o.vertex);
			o.uv = v.uv;
			return o;
		}

		struct fragOut
		{
			half4 col : SV_Target;
		};

			
			sampler2D _MainTex;
			float2 _MainTex_TexelSize;
			float4 _MainTex_ST;

			float _YFlipOverride;
			float _XFlipOverride;

			float _Depth;
			/*
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}*/

			
			fragOut frag(v2f i, out float depth : DEPTH) : SV_Target
			{

				fragOut o;

				if (!_YFlipOverride) {
					i.uv.y = 1 - i.uv.y;
				}

				if (_XFlipOverride)
					i.uv.x = 1 - i.uv.x;

				o.col = tex2D(_MainTex, i.uv);

#ifdef UNITY_REVERSED_Z
					depth = 1 - _Depth;
#else
					depth = _Depth;
#endif
				return o;
			}
			ENDCG
		}
	}
}
