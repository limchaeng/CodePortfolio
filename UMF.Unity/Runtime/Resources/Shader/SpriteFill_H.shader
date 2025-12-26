Shader "UMF/Sprites/SpriteFill_H"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HideInInspector] _Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		[Header(UMF)]
		_FillAmount("Fill Amount", Range(0, 1)) = 0
		_ScrollSpeed("Scroll Speed", float) = 0
		[MaterialToggle] _Reverse("Reverse", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment FillSpriteFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc" 

			float _FillAmount;
			float _ScrollSpeed;
			float _Reverse;

			fixed4 FillSpriteFrag(v2f IN) : SV_Target
			{
				float2 scroll = float2(frac(_Time.x * _ScrollSpeed), 0);
				fixed4 col = SampleSpriteTexture(IN.texcoord - scroll) * IN.color;
				col.rgb *= col.a;
				clip(lerp(-IN.texcoord.x + _FillAmount, IN.texcoord.x - (1 - _FillAmount), _Reverse));
				return col;
			}
			ENDCG
		}
	}
}