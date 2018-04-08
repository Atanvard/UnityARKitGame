Shader "Unlit/WriteToDepth"
{
	Properties
	{
		//_Depth("Depth", Range(0,1)) = 0
	}
		SubShader
	{
		Tags{ "Queue" = "Background" } // irrelevant for blit, but useful for testing
		LOD 100

		Stencil{
		Ref 100
		Comp NotEqual
	}
		Pass
	{
		Cull Off
		ZWrite On
		ZTest Always
		//ColorMask 0
		
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}

	//float _Depth;

	fixed4 frag(v2f i, out float out_depth : SV_Depth) : SV_Target
	{
		out_depth = 1;
		return 0;
	}
		ENDCG
	}
	}
}