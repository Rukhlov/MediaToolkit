
Texture2D txInput : register(t0);

SamplerState GenericSampler : register(s0);

struct PS_INPUT
{
	float4 Pos : SV_POSITION;
	float2 Tex : TEXCOORD;
};

struct PS_OUTPUT
{
	float ColorY	: SV_Target0;
	float2 ColorUV	: SV_Target1;
};
/*
	Y  =      (0.257 * R) + (0.504 * G) + (0.098 * B) + 16
	Cr = V =  (0.439 * R) - (0.368 * G) - (0.071 * B) + 128
	Cb = U = -(0.148 * R) - (0.291 * G) + (0.439 * B) + 128
*/
PS_OUTPUT PS(PS_INPUT input) : SV_Target
{
	PS_OUTPUT output;

	float4 InputColor = txInput.Sample(GenericSampler, input.Tex);

	// Range 0-255
	output.ColorY = (0.257f * InputColor.r + 0.504f * InputColor.g + 0.098f * InputColor.b) + 0.0625f;//(16 / 256.0f);
	output.ColorY = clamp(output.ColorY, 0.0f, 255.0f);

	// Range 0-255
	float ColorU = (-0.148f * InputColor.r - 0.291f * InputColor.g + 0.439f * InputColor.b) + 0.5f;//(128.0f / 256.0f);
	float ColorV = (0.439f * InputColor.r - 0.368f * InputColor.g - 0.071f * InputColor.b) + 0.5f;//(128.0f / 256.0f);

	ColorU = clamp(ColorU, 0.0f, 255.0f);
	ColorV = clamp(ColorV, 0.0f, 255.0f);

	output.ColorUV = float2(ColorU, ColorV);

	return output;
}