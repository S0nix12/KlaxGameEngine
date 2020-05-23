cbuffer MatrixBuffer : register(b0)
{
	matrix worldMatrix;
	matrix invTransposeWorldMatrix;
};

cbuffer CameraBuffer : register(b11)
{
	matrix viewProjMatrix;
	float3 cameraPosition;
	float farRange;
}

struct VSInput
{
	float4 position : POSITION;
};

struct PSInput
{
	float4 position : SV_POSITION;
	float3 positionWS : TEXCOORD0;
};

PSInput Vertex(VSInput input)
{
	PSInput output;

	input.position.w = 1.0f;
	output.position = mul(input.position, worldMatrix);
	
	output.positionWS = output.position;
	output.position = mul(output.position, viewProjMatrix);

	return output;
}

float Pixel(PSInput input) : SV_Depth
{
	return length(input.positionWS - cameraPosition) / farRange;
}