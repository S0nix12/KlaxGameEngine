cbuffer MatrixBuffer : register(b0)
{
	matrix worldMatrix;
	matrix invTransposeWorldMatrix;
};

cbuffer CubeCameraBuffer : register(b11)
{
	matrix projMatrix;
	matrix viewMatrix[6];
	
	float3 cubeCenterPos;
	float cubeFarPlane;
}

struct VSInput
{
	float4 position : POSITION;
};

struct VSOutput
{
	float4 position : SV_POSITION;
};

struct GSOutput
{
	float4 position : SV_POSITION;
	float3 positionWS : TEXCOORD0;
	uint RTIndex : SV_RenderTargetArrayIndex;
};

VSOutput Vertex(VSInput input)
{
	VSOutput output;

	input.position.w = 1.0f;
	output.position = mul(input.position, worldMatrix);

	return output;
}

[maxvertexcount(18)]
void Geometry(triangle VSOutput input[3], inout TriangleStream<GSOutput> cubeMapStream)
{
	[unroll]
	for (int i = 0; i < 6; ++i)
	{
		{
			GSOutput output = (GSOutput)0;

			output.RTIndex = i;

			[unroll]
			for (int v = 0; v < 3; ++v)
			{
				float4 worldPosition = input[v].position;
				output.positionWS = worldPosition.xyz;
				
				float4 viewPosition = mul(worldPosition, viewMatrix[i]);
				output.position = mul(viewPosition, projMatrix);

				cubeMapStream.Append(output);
			}

			cubeMapStream.RestartStrip();
		}
	}
}

float Pixel(GSOutput input) : SV_Depth
{
	float distance = length(input.positionWS - cubeCenterPos);
	return distance / cubeFarPlane;
}