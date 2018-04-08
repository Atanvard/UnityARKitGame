// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/HeadsetlessVRPreview"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			sampler2D _LeftEyeTexture;
			sampler2D _RightEyeTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 0;

			if (unity_StereoEyeIndex == 0)
			{
				col = tex2D(_LeftEyeTexture, i.screenPos * float2(2, 1) - float2(1, 0));
			}
			else {
				col = tex2D(_RightEyeTexture, (i.screenPos* float2(2, 1)) + float2(0.5, 0));
			}
				return col;
			}
			ENDCG
		}
	}
}
