#include "Common.hlsl"

PixelShaderInput VSMain(VertexShaderInput vertex)
{
	PixelShaderInput result = (PixelShaderInput)0;
	// Apply WPV matrix transformation
	result.Position = mul(vertex.Position, WorldViewProjection);
	result.Diffuse = vertex.Color;
	result.TextureUV = vertex.TextureUV;

	// Transform normal to world space
	result.WorldNormal = mul(vertex.Normal, (float3x3)WorldInverseTranspose);

	// transform input position to world
	result.WorldPosition = mul(vertex.Position, World).xyz;
	
	return result;
}
