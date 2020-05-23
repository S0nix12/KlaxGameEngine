#define MAX_PER_OBJECT_LIGHTS 6

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

struct Light
{
	float4 position;
	float4 direction;
	float4 color;
	float range;
	float spotAngle;
	float constantAttenuation;
	float linearAttenuation;
	float quadraticAttenuation;
	int lightType; // 0 = Directional 1 = Point 2 = Spot
	bool bEnabled;
	bool bCastShadows;
	
	matrix viewProjection;
	
	int shadowMapRegister;
	
	float3 _padding0;
};

cbuffer ObjectLightBuffer : register(b12)
{
	int numActiveLights;
	float3 _padding1;
	Light lights[MAX_PER_OBJECT_LIGHTS];
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
	
	float4 positionLightSpace[MAX_PER_OBJECT_LIGHTS] : TEXCOORD3;
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
	output.positionWS = worldPos;
	
	// Transform postion to light space for all lights
	[unroll]
	for (int i = 0; i < MAX_PER_OBJECT_LIGHTS; i++)
	{
		output.positionLightSpace[i] = mul(worldPos, lights[i].viewProjection);
	}
    //output.viewDirection = cameraPosition - worldPos.xyz;
    //output.viewDirection = normalize(output.viewDirection);

	return output;
}