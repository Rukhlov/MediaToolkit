
//https://vec3.ca/bicubic-filtering-in-fewer-taps/

Texture2D g_Tex : register(t0); //the texture we're zooming in on
SamplerState g_Lin : register(s0); //a sampler configured for bilinear filtering

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};


//float4 main(VertData input) :SV_Target
//{
//	float2 iTc = input.texCoord;
//
//	 return g_Tex.Sample(g_Lin, iTc);
//
//
//}


float4 main(VertData input) : SV_Target
{
    //get into the right coordinate system
 
	float2 iTc = input.texCoord;

	float2 texSize = float2(640.0f, 360.0f);

    //g_Tex.GetDimensions( texSize.x, texSize.y );

    float2 invTexSize = 1.0 / texSize;
 
    iTc *= texSize;
	
	 //round tc *down* to the nearest *texel center*
 
    float2 tc = floor( iTc - 0.5 ) + 0.5;
	
	 //compute the fractional offset from that texel center
    //to the actual coordinate we want to filter at
 
    float2 f = iTc - tc;
 
    //we'll need the second and third powers
    //of f to compute our filter weights
 
    float2 f2 = f * f;
    float2 f3 = f2 * f;
 
    //compute the filter weights
 
    //float2 w0 = //...
    //float2 w1 = //...
    //float2 w2 = //...
    //float2 w3 = //...
	

	float2 w0 = f2 - 0.5 * (f3 + f);
	float2 w1 = 1.5 * f3 - 2.5 * f2 + 1.0;
	float2 w3 = 0.5 * (f3 - f2);
	float2 w2 = 1.0 - w0 - w1 - w3;


	//get our texture coordinates
 
    float2 tc0 = tc - 1;
    float2 tc1 = tc;
    float2 tc2 = tc + 1;
    float2 tc3 = tc + 2;
 
    /*
        If we're only using a portion of the texture,
        this is where we need to clamp tc2 and tc3 to
        make sure we don't sample off into the unused
        part of the texture (tc0 and tc1 only need to
        be clamped if our subrectangle doesn't start
        at the origin).
    */
 
    //convert them to normalized coordinates
 
	tc0 *= invTexSize;
	tc1 *= invTexSize;
	tc2 *= invTexSize;
	tc3 *= invTexSize;
	
	float2 s0 = w0 + w1;
	float2 s1 = w2 + w3;

	float2 f0 = w1 / (w0 + w1);
	float2 f1 = w3 / (w2 + w3);

	float2 t0 = tc - 1 + f0;
	float2 t1 = tc + 1 + f1;

	//return g_Tex.Sample(g_Lin, iTc);

	return
		(g_Tex.Sample(g_Lin, float2(t0.x, t0.y)) * s0.x
		+ g_Tex.Sample(g_Lin, float2(t1.x, t0.y)) * s1.x) * s0.y
		+ (g_Tex.Sample(g_Lin, float2(t0.x, t1.y)) * s0.x
		+ g_Tex.Sample(g_Lin, float2(t1.x, t1.y)) * s1.x) * s1.y;


	//return
 //       g_Tex.Sample( g_Lin, float2( tc0.x, tc0.y ) ) * w0.x * w0.y
 //     + g_Tex.Sample( g_Lin, float2( tc1.x, tc0.y ) ) * w1.x * w0.y
 //     + g_Tex.Sample( g_Lin, float2( tc2.x, tc0.y ) ) * w2.x * w0.y
 //     + g_Tex.Sample( g_Lin, float2( tc3.x, tc0.y ) ) * w3.x * w0.y
 //
 //     + g_Tex.Sample( g_Lin, float2( tc0.x, tc1.y ) ) * w0.x * w1.y
 //     + g_Tex.Sample( g_Lin, float2( tc1.x, tc1.y ) ) * w1.x * w1.y
 //     + g_Tex.Sample( g_Lin, float2( tc2.x, tc1.y ) ) * w2.x * w1.y
 //     + g_Tex.Sample( g_Lin, float2( tc3.x, tc1.y ) ) * w3.x * w1.y
 //
 //     + g_Tex.Sample( g_Lin, float2( tc0.x, tc2.y ) ) * w0.x * w2.y
 //     + g_Tex.Sample( g_Lin, float2( tc1.x, tc2.y ) ) * w1.x * w2.y
 //     + g_Tex.Sample( g_Lin, float2( tc2.x, tc2.y ) ) * w2.x * w2.y
 //     + g_Tex.Sample( g_Lin, float2( tc3.x, tc2.y ) ) * w3.x * w2.y
 //
 //     + g_Tex.Sample( g_Lin, float2( tc0.x, tc3.y ) ) * w0.x * w3.y
 //     + g_Tex.Sample( g_Lin, float2( tc1.x, tc3.y ) ) * w1.x * w3.y
 //     + g_Tex.Sample( g_Lin, float2( tc2.x, tc3.y ) ) * w2.x * w3.y
 //     + g_Tex.Sample( g_Lin, float2( tc3.x, tc3.y ) ) * w3.x * w3.y;
}