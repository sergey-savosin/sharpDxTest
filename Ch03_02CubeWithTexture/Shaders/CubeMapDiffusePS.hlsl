// Globals for texture sampling
TextureCube CubeMap : register(t0);
SamplerState SamplerClamp : register(s0);

#include "Common.hlsl"

// A basic diffuse lighting pixel shader
float4 PSMain(PixelShaderInput pixel) : SV_Target
{
	// Important: After interpolation the values are not necessarily normalized
	float3 normal = normalize(pixel.WorldNormal);
	float3 toEye = normalize(CameraPosition - pixel.WorldPosition);
	float3 toLight = normalize(-Light.Direction);

	// Texture sample here (use white if no texture)
	float4 sample = CubeMap.Sample(SamplerClamp, normal);

	float3 ambient = MaterialAmbient.rgb;
	float3 emissive = MaterialEmissive.rgb;
	float3 diffuse = Lambert(pixel.Diffuse, normal, toLight);

	// Calculate final color component
	float3 color = (saturate(ambient + diffuse) * sample.rgb) * Light.Color.rgb + emissive;
	// We saturate ambient+diffuse to ensure there is no over-
	// brightness on the texture sample if the sum is greater than 1

	// Calculate final alpha value
	float alpha = pixel.Diffuse.a * sample.a;

	// Return result
	return float4(color, alpha);
}