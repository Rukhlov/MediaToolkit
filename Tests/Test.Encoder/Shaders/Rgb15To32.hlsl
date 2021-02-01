
Texture2D<float>  rgb16Texture : t0;
SamplerState      defaultSampler : s0;

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};

float3 Rgb555ToRgb888(float rgb555) {
	/* rgb555 -> rgb888
		WORD red_mask = 0x7C00;
		WORD green_mask = 0x3E0;
		WORD blue_mask = 0x1F;

		BYTE red_value = (pixel & red_mask) >> 10;
		BYTE green_value = (pixel & green_mask) >> 5;
		BYTE blue_value = (pixel & blue_mask);

		// Expand to 8-bit values:
		BYTE red   = red_value << 3;
		BYTE green = green_value << 3;
		BYTE blue  = blue_value << 3;
	*/

	uint i = rgb555 * 0xFFFF;
	uint r = ((i & 0x7C00) >> 10) << 3;
	uint g = ((i & 0x3E0) >> 5) << 3;
	uint b = ((i & 0x1F) << 3);
	return float3(r / 255.0, g / 255.0, b / 255.0);

}

float4 PS(VertData input) : SV_Target
{
	float rgb16 = rgb16Texture.Sample(defaultSampler, input.texCoord);

	return float4(saturate(Rgb555ToRgb888(rgb16)), 1.0);

}
