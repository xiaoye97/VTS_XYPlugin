/*
	Simple box blur shader for UI elements, blurring the background.

	This variant uses independent grab passes to avoid glitches that occur when
	elements touch viewport borders.

	This also means it will work with multiple blurry widgets set on top of each
	other.
*/

Shader "UI/Blur" {
Properties {
	[HideInInspector]
	_MainTex("", 2D) = "" {}
	_Opacity("Opacity", Range(0.0, 1.0)) = 0.5
	_Size("Size", Range(1.0, 16.0)) = 4.0
}
SubShader {
	Tags {
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
	}
	Cull Off
	ZTest [unity_GUIZTestMode]
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha

	CGINCLUDE

#include "UIBlur.cginc"

sampler2D _GrabTexture;

float4 PS_BlurX(
	float4 p : SV_POSITION,
	float2 uv1 : TEXCOORD0,
	float4 uv2 : TEXCOORD1
) : SV_TARGET {
	return blur_x(uv1, uv2, _GrabTexture);
}

float4 PS_BlurY(
	float4 p : SV_POSITION,
	float2 uv1 : TEXCOORD0,
	float4 uv2 : TEXCOORD1,
	float4 img_color : COLOR
) : SV_TARGET {
	return blur_y(uv1, uv2, img_color, _GrabTexture);
}

	ENDCG

	GrabPass {
		Tags {
			"LightMode" = "Always"
		}
	}

	Pass {
		CGPROGRAM
		#pragma vertex VS_QuadProj
		#pragma fragment PS_BlurX
		ENDCG
	}

	GrabPass {
		Tags {
			"LightMode" = "Always"
		}
	}

	Pass {
		CGPROGRAM
		#pragma vertex VS_QuadProjColor
		#pragma fragment PS_BlurY
		ENDCG
	}
}
}