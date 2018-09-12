// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FY/GuiGray Mask (AlphaClip)"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Blend ("Blend", Float) = 0.0
		_MaskTex ("Alpha (A)", 2D) = "white" {}
		_Offset ("Offset", Vector) = (0.0, 0.0, 1.0, 1.0)
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Blend;
			float4 _MainTex_ST;
			sampler2D _MaskTex;
			float4 _Offset;

			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
#if defined(UNITY_HALF_TEXEL_OFFSET)
				o.vertex.xy = o.vertex.xy + float2(-1.0, 1.0) / _ScreenParams.xy;
#endif						
				o.worldPos = TRANSFORM_TEX(v.vertex.xy, _MainTex);
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				// Sample the texture
				half4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
				col.a = col.a * tex2D(_MaskTex, float2((IN.texcoord.x - _Offset.x) / (_Offset.z - _Offset.x), (IN.texcoord.y - _Offset.y) / (_Offset.w - _Offset.y))).r;
				
				float grayscale = Luminance(col.rgb);
				float4 output = float4(grayscale, grayscale, grayscale, col.a); 
				
				output.rgb = (1.0 - _Blend) * output.rgb + _Blend * col.rgb;
				col = output;

				float2 factor = abs(IN.worldPos);
				float val = 1.0 - max(factor.x, factor.y);

				// Option 1: 'if' statement
				if (val < 0.0) col.a = 0.0;

				// Option 2: no 'if' statement -- may be faster on some devices
				//col.a *= ceil(clamp(val, 0.0, 1.0));

				return col;
			}
			ENDCG
		}
	}
	
	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			ColorMask RGB
			AlphaTest Greater .01
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}