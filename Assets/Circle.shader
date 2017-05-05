Shader "Unlit/Circle"
{
	Properties
	{
		CircleColour ("Colour", Color) = (1,1,1,1)
		MaxRadius ("Max Radius", Float) = 1.0
		MinRadius ("Min Radius", Float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float4 vertex : SV_POSITION;
			};

			float MaxRadius;
			float MinRadius;

			float4 CircleColour;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float radius = length(i.uv - float2(.5, .5));

				if(radius < MinRadius || radius > MaxRadius)
				{
					discard;
				}
				return CircleColour;
			}
			ENDCG
		}
	}
}
