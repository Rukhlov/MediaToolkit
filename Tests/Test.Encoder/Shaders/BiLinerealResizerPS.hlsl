sampler s0 : register(s0);
sampler s1 : register(s1);
sampler s2 : register(s2);
sampler s3 : register(s3);
sampler s4 : register(s4);

Texture2D<float>  tex : t0;


float4 dxdy05 : register(c0);
float2 dxdy :   register(c1);
float2 dx :     register(c2);
float2 dy :     register(c3);

#define A _The_Value_Of_A_Is_Set_Here_

// none of the resizers here can be used for 1:1 mapping!
// tex * size won't be 0, 1, 2, 3, ... as you might expect, but something like 0, 0.999, 2.001, 2.999, ...
// this means when the fractional part becomes 0.999 we will be interpolating with the wrong value!!!

struct PS_INPUT 
{
	float2 t0 : TEXCOORD0;
	float2 t1 : TEXCOORD1;
	float2 t2 : TEXCOORD2;
	float2 t3 : TEXCOORD3;
	float2 t4 : TEXCOORD4;
};

float4 main_bilinear(PS_INPUT input) : SV_Target
{
	float2 PixelPos = input.t0;
	float2 dd = frac(PixelPos);
	float2 ExactPixel = PixelPos - dd;
	float2 samplePos = ExactPixel * dxdy + dxdy05;

	float4 x = lerp(tex.Sample(s0, samplePos), tex.Sample(s0, samplePos + dx), dd.x);
	float4 y = lerp(tex.Sample(s0, samplePos + dy), tex.Sample(s0, samplePos + dxdy), dd.x);

	float4 c = lerp(x, y, dd.y);

	return c;
}



//sampler s0 : register(s0);
//sampler s1 : register(s1);
//sampler s2 : register(s2);
//sampler s3 : register(s3);
//sampler s4 : register(s4);
//
//float4 dxdy05 : register(c0);
//float2 dxdy :   register(c1);
//float2 dx :     register(c2);
//float2 dy :     register(c3);
//
//#define A _The_Value_Of_A_Is_Set_Here_
//
//// none of the resizers here can be used for 1:1 mapping!
//// tex * size won't be 0, 1, 2, 3, ... as you might expect, but something like 0, 0.999, 2.001, 2.999, ...
//// this means when the fractional part becomes 0.999 we will be interpolating with the wrong value!!!
//
//struct PS_INPUT {
//	float2 t0 : TEXCOORD0;
//	float2 t1 : TEXCOORD1;
//	float2 t2 : TEXCOORD2;
//	float2 t3 : TEXCOORD3;
//	float2 t4 : TEXCOORD4;
//};
//
//float4 main_bilinear(PS_INPUT input) : COLOR 
//{
//	float2 PixelPos = input.t0;
//	float2 dd = frac(PixelPos);
//	float2 ExactPixel = PixelPos - dd;
//	float2 samplePos = ExactPixel * dxdy + dxdy05;
//
//	float4 x = lerp(tex2D(s0, samplePos), tex2D(s0, samplePos + dx), dd.x);
//	float4 y = lerp(tex2D(s0, samplePos + dy), tex2D(s0, samplePos + dxdy), dd.x);
//
//	float4 c = lerp(x, y, dd.y);
//
//	return c;
//}