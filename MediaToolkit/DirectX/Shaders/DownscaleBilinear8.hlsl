
Texture2D image				 : register(t0); 
SamplerState textureSampler  : register(s0);


//SamplerState textureSampler {
//	Filter = Linear;
//	AddressU = Clamp;
//	AddressV = Clamp;
//};

struct VertData {
	float4 pos : SV_Position;
	float2 uv  : TEXCOORD0;
};

float4 pixel(float2 uv)
{
	return image.Sample(textureSampler, uv);
}

float4 DrawLowresBilinear(VertData v_in)
{
	float2 uv = v_in.uv;

	float2 stepxy = float2(ddx(uv.x), ddy(uv.y));
	float2 stepxy1 = stepxy * 0.0625;
	float2 stepxy3 = stepxy * 0.1875;
	float2 stepxy5 = stepxy * 0.3125;
	float2 stepxy7 = stepxy * 0.4375;

	// Simulate Direct3D 8-sample pattern
	float4 out_color;
	out_color = pixel(uv + float2(stepxy1.x, -stepxy3.y));
	out_color += pixel(uv + float2(-stepxy1.x, stepxy3.y));
	out_color += pixel(uv + float2(stepxy5.x, stepxy1.y));
	out_color += pixel(uv + float2(-stepxy3.x, -stepxy5.y));
	out_color += pixel(uv + float2(-stepxy5.x, stepxy5.y));
	out_color += pixel(uv + float2(-stepxy7.x, -stepxy1.y));
	out_color += pixel(uv + float2(stepxy3.x, stepxy7.y));
	out_color += pixel(uv + float2(stepxy7.x, -stepxy7.y));

	return out_color * 0.125;
}

float4 PS(VertData v_in) : SV_Target
{
	return DrawLowresBilinear(v_in);
}

