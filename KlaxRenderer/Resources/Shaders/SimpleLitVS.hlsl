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
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
    float3 viewDirection : TEXCOORD1;
    float4 positionWS : TEXCOORD2;
};

PSInput Vertex(VSInput input)
{
    PSInput output;

    input.position.w = 1.0f;
    matrix worldViewProj = mul(worldMatrix, viewProjMatrix);
    output.position = mul(input.position, worldViewProj);

    output.texCoord = input.texCoord;
    output.normal = normalize(mul(input.normal, (float3x3) invTransposeWorldMatrix));
    //output.normal = input.normal;

    float4 worldPos = mul(input.position, worldMatrix);
    output.viewDirection = cameraPosition;
    output.positionWS = mul(input.position, worldMatrix);
    //output.viewDirection = cameraPosition - worldPos.xyz;
    //output.viewDirection = normalize(output.viewDirection);

    return output;
}