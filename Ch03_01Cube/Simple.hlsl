// Constant buffer to be updated...
cbuffer PerObject : register(b0)
{
	// WorldViewProjection matrix
	float4x4 WorldViewProj;
};

// Vertex shader input...
struct VertexShaderInput
{
	float4 Position : SV_Position;
	float4 Color : COLOR;
};

// Vertex shader output...
struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR;
};

// Vertex shader main function
VertexShaderOutput VSMain(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Transform
	output.Position = mul(input.Position, WorldViewProj);
	// Pass through the color
	output.Color = input.Color;

	return output;
}

// A simple Pixel Shader
float4 PSMain(VertexShaderOutput input) : SV_Target
{
	return input.Color;
}
