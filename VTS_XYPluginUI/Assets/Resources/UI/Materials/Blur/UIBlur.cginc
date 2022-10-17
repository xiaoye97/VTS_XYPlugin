#ifndef UI_BLUR_CGINC
#define UI_BLUR_CGINC

#include "UnityCG.cginc"

// Pixel size.
static const float2 ps = _ScreenParams.zw - 1.0;

// Parameters.
sampler2D _MainTex;
float _Opacity, _Size;

// Functions.

// Creates a linear blur from a projected texture.
// The blur is made centered, so the direction is always absolute.
// sp - Texture sampler.
// uv - Texture coordinates.
// dir - Blur direction vector.
float4 linear_blur(sampler2D sp, float4 uv, float2 dir) {
	static const int samples = 9;

	float4 color = 0.0;

	// Move coordinates in opposite direction to center the sampling.
	uv = UNITY_PROJ_COORD(float4(
		uv.x - dir.x * samples * 0.5,
		uv.y - dir.y * samples * 0.5,
		uv.z,
		uv.w
	));

	[unroll]
	for (int i = 0; i < samples; ++i) {
		uv = UNITY_PROJ_COORD(float4(
			uv.x + dir.x,
			uv.y + dir.y,
			uv.z,
			uv.w
		));
		color += tex2Dproj(sp, uv);
	}

	return color / samples;
}

// Vertex shaders.

void VS_Quad(
	float4 v : POSITION,
	out float4 p : SV_POSITION,
	inout float2 uv : TEXCOORD
) {
	p = UnityObjectToClipPos(v);
}

void VS_QuadProj(
    float4 v : POSITION,
    out float4 p : SV_POSITION,
    inout float2 uv1 : TEXCOORD0,
	out float4 uv2 : TEXCOORD1
) {
    VS_Quad(v, p, uv1);
	uv2 = ComputeGrabScreenPos(p);
}

void VS_QuadProjColor(
	float4 v : POSITION,
	out float4 p : SV_POSITION,
	inout float2 uv1 : TEXCOORD0,
	out float4 uv2 : texcoord2,
	inout float4 img_color : COLOR
) {
	VS_QuadProj(v, p, uv1, uv2);
}

// Pixel shader functions (for changing the grab pass texture).

float4 blur_x(float2 img_uv, float4 grab_uv, sampler2D grab_tex) {
	float2 dir = float2(ps.x * _Size, 0.0);
    
	float4 blur = linear_blur(grab_tex, grab_uv, dir);
	blur.a = 1.0;

	float4 color = tex2D(_MainTex, img_uv);

    return blur * color.a;
}

float4 blur_y(float2 img_uv, float4 grab_uv, float4 img_color, sampler2D grab_tex) {
	float2 dir = float2(0.0, ps.y * _Size);
    
	float4 blur = linear_blur(grab_tex, grab_uv, dir);
	blur.a = 1.0;

	float4 color = tex2D(_MainTex, img_uv) * img_color;
	color = lerp(blur * color.a, color, _Opacity);

	return color;
}

#endif