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
	float4 color : COLOR;
};

struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PSInput Vertex(VSInput input)
{
	PSInput output;

	input.position.w = 1.0f;
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewProjMatrix);

	output.color = input.color;

	return output;
}

float4 Pixel(PSInput input) : SV_TARGET
{
	return input.color;
}