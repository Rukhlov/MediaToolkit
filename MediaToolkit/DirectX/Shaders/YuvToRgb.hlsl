Texture2D<float>  lumaTex : t0;
Texture2D<float> chromaCbTex	: t1;
Texture2D<float> chromaCrTex	: t2;
SamplerState      defaultSampler : s0;

cbuffer dataBuffer : register(b0)
{
	float4x4 colorMatrix;
};

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};


float4 PS(VertData input) : SV_Target
{
	float y = lumaTex.Sample(defaultSampler, input.texCoord);
	float u = chromaCbTex.Sample(defaultSampler, input.texCoord);
	float v = chromaCrTex.Sample(defaultSampler, input.texCoord);

	float4 yuvx = float4(y, u, v, 1.0);
	float4 rgba = mul(yuvx, colorMatrix);

	return float4(saturate(rgba.rgb), 1.0);

	// return float4(saturate(rgba.zxy), 1.0);
}