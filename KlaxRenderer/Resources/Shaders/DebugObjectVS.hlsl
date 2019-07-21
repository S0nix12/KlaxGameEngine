

cbuffer CameraBuffer : register(b11)
{
    matrix viewProjMatrix;
    float3 cameraPosition;
}

struct VSInput
{
    float4 position : POSITION;
    float4 color : INSTANCE_COLOR;
    matrix world : INSTANCE_TRANSFORM;
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
    matrix worldViewProjection = mul(input.world, viewProjMatrix);
    output.position = mul(input.position, worldViewProjection);
    output.color = input.color;

    return output;
}