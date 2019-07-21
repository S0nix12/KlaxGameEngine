

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

    // We assume our line vertices are already in world space
    output.position = mul(input.position, viewProjMatrix);
    output.color = input.color;

    return output;
}