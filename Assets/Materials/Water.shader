Shader "Custom/Water" {
	Properties {
		_LowTideDistance("Low Tide Distance", Float) = 0.7
		_HighTideDistance("High Tide Distance", Float) = 0.85
		_EdgeColour("Edge Colour", Color) = (1,1,1,1)
		_MainColour("Main Colour", Color) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float4 vertColour : COLOR;
		};

		float _LowTideDistance;
		float _HighTideDistance;
		fixed4 _EdgeColour;
		fixed4 _MainColour;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float height = IN.vertColour.x;
			float tide = (_SinTime.w * 0.5 + 0.5) * (_HighTideDistance - _LowTideDistance) + _LowTideDistance;
			if (height < tide) {
				o.Albedo = _MainColour.rgb;
			}
			else
			{
				float alpha = (height - tide) / (1 - tide);
				o.Albedo = (_EdgeColour.rgb - _MainColour.rgb) * alpha + _MainColour.rgb;
			}
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
