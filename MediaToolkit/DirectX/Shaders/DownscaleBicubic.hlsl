﻿
Texture2D diffuseTexture : register(t0);
SamplerState textureSampler  : register(s0);

cbuffer dataBuffer : register(b0)
{
	float2 baseDimensionI;
};

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};

float weight(float x)
{
	float ax = abs(x);
	// Sharper version.
	// May look better in some cases.
	const float B = 0.0;
	const float C = 0.75;

	if (ax < 1.0)
		return (pow(x, 2.0) * ((12.0 - 9.0 * B - 6.0 * C) * ax + (-18.0 + 12.0 * B + 6.0 * C)) + (6.0 - 2.0 * B)) / 6.0;
	else if ((ax >= 1.0) && (ax < 2.0))
		return (pow(x, 2.0) * ((-B - 6.0 * C) * ax + (6.0 * B + 30.0 * C)) + (-12.0 * B - 48.0 * C) * ax + (8.0 * B + 24.0 * C)) / 6.0;
	else
		return 0.0;
}

float4 weight4(float x)
{
	return float4(
		weight(x - 2.0),
		weight(x - 1.0),
		weight(x),
		weight(x + 1.0));
}

float3 pixel(float xpos, float ypos)
{
	return diffuseTexture.Sample(textureSampler, float2(xpos, ypos)).rgb;
}

float3 get_line(float ypos, float4 xpos, float4 linetaps)
{
	return
		pixel(xpos.r, ypos) * linetaps.r +
		pixel(xpos.g, ypos) * linetaps.g +
		pixel(xpos.b, ypos) * linetaps.b +
		pixel(xpos.a, ypos) * linetaps.a;
}

float4 main(VertData input) : SV_Target
{
	//float2 stepxy = float2(0.00005,0.00009);
	float2 stepxy = baseDimensionI;

	float2 pos = input.texCoord + stepxy * 0.5;
	float2 f = frac(pos / stepxy);

	float4 linetaps = weight4(1.0 - f.x);
	float4 columntaps = weight4(1.0 - f.y);

	//make sure all taps added together is exactly 1.0, otherwise some (very small) distortion can occur
	linetaps /= linetaps.r + linetaps.g + linetaps.b + linetaps.a;
	columntaps /= columntaps.r + columntaps.g + columntaps.b + columntaps.a;

	float2 xystart = (-1.5 - f) * stepxy + pos;
	float4 xpos = float4(xystart.x, xystart.x + stepxy.x, xystart.x + stepxy.x * 2.0, xystart.x + stepxy.x * 3.0);

	float4 rgba;
	rgba.rgb =
		get_line(xystart.y                 , xpos, linetaps) * columntaps.r +
		get_line(xystart.y + stepxy.y      , xpos, linetaps) * columntaps.g +
		get_line(xystart.y + stepxy.y * 2.0, xpos, linetaps) * columntaps.b +
		get_line(xystart.y + stepxy.y * 3.0, xpos, linetaps) * columntaps.a;

	rgba.a = 1.0;

	return rgba;

}
