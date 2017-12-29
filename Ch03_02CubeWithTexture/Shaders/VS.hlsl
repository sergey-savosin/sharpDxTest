#include "Common.hlsl"

PixelShaderInput VSMain(VertexShaderInput vertex)
{
	PixelShaderInput result = (PixelShaderInput)0;
	
	vertex.Position.w = 1.0; //!
	
	// Apply WPV matrix transformation
	result.Position = mul(vertex.Position, WorldViewProjection);
	result.Diffuse = vertex.Color * MaterialDiffuse;
	result.TextureUV = vertex.TextureUV;

	// Transform normal to world space
	result.WorldNormal = mul(vertex.Normal, (float3x3)WorldInverseTranspose);

	// transform input position to world
	result.WorldPosition = mul(vertex.Position, World).xyz;

	// Apply material UV transformation
	result.TextureUV = mul(float4(vertex.TextureUV.x, vertex.TextureUV.y, 0, 1), (float4x2)UVTransform).xy;
	
	return result;
}
