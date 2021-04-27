
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

float4 main(VertData input) : SV_Target
{
	float2 texCoord = input.texCoord;

	float2 adjust = baseDimensionI;
	//float2 adjust = float2(0.00005, 0.00009);
	float4 rgba;
	rgba.rgb = diffuseTexture.Sample(textureSampler, texCoord).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(-adjust.x, -adjust.y)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(-adjust.x,       0.0)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(-adjust.x,  adjust.y)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(0.0, -adjust.y)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(0.0,  adjust.y)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(adjust.x, -adjust.y)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(adjust.x,       0.0)).rgb;
	rgba.rgb += diffuseTexture.Sample(textureSampler, texCoord + float2(adjust.x,  adjust.y)).rgb;
	rgba.rgb /= 9.0;

	rgba.a = 1.0;

	return rgba;

}