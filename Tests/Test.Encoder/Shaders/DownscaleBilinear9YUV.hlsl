uniform Texture2D diffuseTexture;
uniform float2 baseDimensionI = float2(1.0 / 1920.0, 1.0 / 1080.0);

//uniform float4x4 yuvMat;
//uniform float2 baseDimensionI;

SamplerState textureSampler
{
	AddressU = Clamp;
	AddressV = Clamp;
	Filter = Linear;
};

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};

float4 main(VertData input) : SV_Target
{
	float2 texCoord = input.texCoord;

	//float2 adjust = baseDimensionI;
	float2 adjust = float2(0.00005, 0.00009);
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

	////-------------------------------------------------------------

	////a nice quick colorspace conversion
	//float4 yuvx = mul(float4(rgba.rgb, 1.0), yuvMat);
	//return float4(saturate(yuvx.zxy), rgba.a);
}