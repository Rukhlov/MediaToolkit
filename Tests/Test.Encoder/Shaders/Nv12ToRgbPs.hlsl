
Texture2D<float>  txLuma			: t0;
Texture2D<float2> txChroma			: t1;
SamplerState      defaultSampler	: s0;

struct PS_INPUT
{
	min16float4 pos         : SV_POSITION;
	min16float2 texCoord    : TEXCOORD0;
};


// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
// Section: Converting 8-bit YUV to RGB888
/*
	C = Y - 16
	D = U - 128
	E = V - 128

	R = clip( round( 1.164383 * C                   + 1.596027 * E  ) )
	G = clip( round( 1.164383 * C - (0.391762 * D) - (0.812968 * E) ) )
	B = clip( round( 1.164383 * C +  2.017232 * D                   ) )
*/
static const float3x3 YUVtoRGBCoeffMatrix = 
{
    1.164383f,  1.164383f, 1.164383f,
    0.000000f, -0.391762f, 2.017232f,
    1.596027f, -0.812968f, 0.000000f
};

float3 ConvertYUVtoRGB(float3 yuv)
{
    // (16 / 255)  (128 / 255) (128 / 255)
    yuv -= float3(0.062745f, 0.501960f, 0.501960f);
    yuv = mul(yuv, YUVtoRGBCoeffMatrix);

    return saturate(yuv);
}

min16float4 PS(PS_INPUT input) : SV_TARGET
{
    float y = txLuma.Sample(defaultSampler, input.texCoord);
    float2 uv = txChroma.Sample(defaultSampler, input.texCoord);
	float3 rgb = ConvertYUVtoRGB(float3(y, uv));
    return min16float4(rgb, 1.f);
}
