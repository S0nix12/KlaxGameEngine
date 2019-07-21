cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix invTransposeWorldMatrix;
};

cbuffer CameraBuffer : register(b11)
{
    matrix viewProjMatrix;
    float3 cameraPosition;
}

struct VSInput
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD0;
};

struct PSInput
{
	float4 position : SV_POSITION;
	float2 texCoord : TEXCOORD0;
};

PSInput Vertex(VSInput input)
{
	PSInput output;

	input.position.w = 1.0f;
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewProjMatrix);

	output.texCoord = input.texCoord;

	return output;
}

/////////////
// Pixel Shader Variables
////////////

cbuffer ColorBuffer : register(b1)
{
    float4 color;
}

Texture2D shaderTexture : register(ps, t0);
SamplerState samplerType : register(ps, s0);

float4 Pixel(PSInput input) : SV_TARGET
{
    float4 textureColor = shaderTexture.Sample(samplerType, input.texCoord);
    return textureColor * color;
}