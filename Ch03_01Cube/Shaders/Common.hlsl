struct DirectionalLight
{
	float4 Color;
	float3 Direction;
};

// Vertex shader input...
struct VertexShaderInput
{
	float4 Position : SV_Position;
	float3 Normal : NORMAL; // normal for lighting
	float4 Color : COLOR; // Vertex color
	float2 TextureUV : TEXCOORD; // texture UV coordinate
};

struct PixelShaderInput
{
	float4 Position : SV_Position;
	// interpolation of vertex * material diffuse
	float4 Diffuse : COLOR;
	// interpolation of vertex UV texture coordinate
	float2 TextureUV : TEXCOORD;

	// we need the World Position and normal for lighting
	float3 WorldNormal : NORMAL;
	float3 WorldPosition : WORLDPOS;
};

// Constant buffer to be updated by application per object
cbuffer PerObject : register(b0)
{
	// WorldViewProjection matrix
	float4x4 WorldViewProjection;

	float4x4 World;

	float4x4 WorldInverseTranspose;
};

cbuffer PerFrame : register(b1)
{
	DirectionalLight Light;
	float3 CameraPosition;
};

cbuffer PerMaterial : register(b2)
{
	float4 MaterialAmbient;
	float4 MaterialDiffuse;
	float4 MaterialSpecular;
	float MaterialSpecularPower;
	bool HasTexture;
	float4 MaterialEmissive;
	float4x4 UVTransform;

}