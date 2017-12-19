#include "Common.hlsl"

// a simple Pixel Shader that simply passes through the
// interpolated color
float4 PSMain(PixelShaderInput pixel) : SV_Target
{
	return pixel.Diffuse;
}
