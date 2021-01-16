
Texture2D inputTex : register(t0);

SamplerState defaultSampler : register(s0);

cbuffer dataBuffer : register(b0)
{
	float4x4 colorMatrix;
};

struct PS_INPUT
{
	float4 Pos : SV_POSITION;
	float2 Tex : TEXCOORD;
};

struct PS_OUTPUT
{
	float ColorY	: SV_Target0;
	float2 ColorU	: SV_Target1;
	float2 ColorV	: SV_Target2;
};


PS_OUTPUT PS(PS_INPUT input) : SV_Target
{
	PS_OUTPUT output;

	float4 rgba = inputTex.Sample(defaultSampler, input.Tex);

	float4 yuvx = mul(float4(rgba.rgb, 1.0), colorMatrix);
	float3 yuv = saturate(yuvx.rgb);

	output.ColorY = float(yuv.r);
	output.ColorU = float(yuv.g);
	output.ColorV = float(yuv.b);

	return output;
}