#include "Common.hlsl"

float4 PSMain(PixelShaderInput pixel) : SV_Target
{
	// Take (z/w) and use as color, this gives the depth
	float4 output = float4(pixel.Position.z, 0, 0, 1);
	return output;
}
