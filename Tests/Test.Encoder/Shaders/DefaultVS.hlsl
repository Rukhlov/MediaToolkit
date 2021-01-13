
//uniform float4x4 ViewProj;

cbuffer dataBuffer : register(b0)
{
	float4x4 ViewProj;
};

struct VS_INPUT
{
	float4 Pos : POSITION;
	float2 Tex : TEXCOORD;
};

struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float2 Tex : TEXCOORD;
};
//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT input)
{
	//VS_OUTPUT output;
	//output.Pos = mul(float4(input.Pos.xyz, 1.0), ViewProj);
	//output.Tex = input.Tex;
	//return output;

	return input;
}

//static  float2 base_dimension = float2(1920.0, 1080.0);
//
//struct VertData {
//	float4 pos : POSITION;
//	float2 uv  : TEXCOORD0;
//};
//
//struct VertOut {
//	float2 uv  : TEXCOORD0;
//	float4 pos : POSITION;
//};
//
//VertOut VS(VertData v_in)
//{
//	VertOut vert_out;
//	vert_out.uv = v_in.uv * base_dimension;
//	vert_out.pos = float4(v_in.pos.xyz, 1.0);
//	return vert_out;
//}

