/////////////
// Pixel Shader Variables
////////////

#define MAX_PER_OBJECT_LIGHTS 6

// Light types.
#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

struct PSInput
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
    float3 viewDirection : TEXCOORD1;
    float4 positionWS : TEXCOORD2;
};

struct DirLight
{
    float4 color;
    float3 direction;
    float _padding;
};

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
    float _padding;
};

cbuffer MaterialDescription : register(b1)
{
    float4 diffuseTextureTint;
    float4 specularColor;
    float specularPower;
    float3 _padding0;
}

cbuffer ObjectLightBuffer : register(b12)
{
    int numActiveLights;
    float3 _padding1;
    Light lights[MAX_PER_OBJECT_LIGHTS];
}

cbuffer SharedLightBuffer : register(b13)
{
    float4 ambientColor;
    DirLight directionalLight;
}

Texture2D shaderTexture : register(ps, t0);
SamplerState samplerType : register(ps, s0);

struct LightingResult
{
    float4 diffuseColor;
    float4 specularColor;
};

LightingResult CalcLight(float4 lightColor, float3 lightDir, float3 normal, float3 viewDirection)
{
    LightingResult outResult;
    float diffuseIntensity = saturate(dot(normal, lightDir));

	//Phon lighting
    float3 reflected = normalize(reflect(-lightDir, normal));
    float RdotV = max(0, dot(reflected, viewDirection));
    float specularIntensity = pow(RdotV, specularPower);

	// Blinn-Phong lighting (simplified phong)
    //float3 halfAngle = normalize(lightDir + viewDirection);
    //float normalDot = max(0, dot(normal, halfAngle));
    //float specularIntensity = pow(normalDot, specularPower);

    outResult.diffuseColor = lightColor * diffuseIntensity;
    outResult.specularColor = lightColor * specularIntensity;

    return outResult;
}

LightingResult CalcDirectionalLight(DirLight light, float3 normal, float3 viewDirection)
{
    return CalcLight(light.color, -light.direction, normal, viewDirection);
	// Invert light direction
    //LightingResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    //float3 lightDir = -light.direction;
    //float intensity = saturate(dot(normal, lightDir));

    //float3 H = normalize(lightDir + viewDirection);
    //float NdotH = max(0, dot(normal, H));
    //float specularIntensity = pow(NdotH, 150.0f);

    //result.diffuseColor = saturate(light.color * (intensity + specularIntensity));
    //return result;
}

LightingResult CalcPointLight(Light pointLight, float3 normal, float3 viewDirection, float3 positionWS)
{
    LightingResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    float3 lightDirection = pointLight.position.xyz - positionWS;
    float distance = length(lightDirection);

	if(distance > pointLight.range)
    {
        return result;
    }

    lightDirection /= distance;

    result = CalcLight(pointLight.color, lightDirection, normal, viewDirection);
    float attenuation = pointLight.constantAttenuation + pointLight.linearAttenuation * distance + pointLight.quadraticAttenuation * distance * distance;

    result.diffuseColor /= attenuation;
    result.specularColor /= attenuation;

    return result;
}

LightingResult CalcSpotLight(Light spotLight, float3 normal, float3 viewDirection, float3 positionWS)
{
    LightingResult result = CalcPointLight(spotLight, normal, viewDirection, positionWS);

    float3 lightDirection = spotLight.position.xyz - positionWS;
    lightDirection = normalize(lightDirection);
    float minCos = cos(spotLight.spotAngle);
    float maxCos = (minCos + 1.0f) / 2.0; // TODO henning replace with inner and outer spot angle
    float cosAngle = dot(spotLight.direction.xyz, -lightDirection);
    float coneIntensity = smoothstep(minCos, maxCos, cosAngle);

    result.diffuseColor *= coneIntensity;
    result.specularColor *= coneIntensity;

    return result;
}

float4 Pixel(PSInput input) : SV_TARGET
{
    float4 textureColor = shaderTexture.Sample(samplerType, input.texCoord);
    if(textureColor.a <= 0.1f)
    {
        discard;
    }

    float3 viewDirection = normalize(input.viewDirection - input.positionWS.xyz);
    input.normal = normalize(input.normal);
    LightingResult lighting = CalcDirectionalLight(directionalLight, input.normal, viewDirection);
    
    for (int i = 0; i < numActiveLights; ++i)
    {
        LightingResult lightResult = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
        if (!lights[i].bEnabled)
            continue;

        switch (lights[i].lightType)
        {
            case DIRECTIONAL_LIGHT:
                break;
            case POINT_LIGHT:
                lightResult = CalcPointLight(lights[i], input.normal, viewDirection, input.positionWS.xyz);
                break;
            case SPOT_LIGHT:
                lightResult = CalcSpotLight(lights[i], input.normal, viewDirection, input.positionWS.xyz);
                break;
        }
        lighting.diffuseColor += lightResult.diffuseColor;
        lighting.specularColor += lightResult.specularColor;
    }

    lighting.diffuseColor = saturate(lighting.diffuseColor);
    lighting.specularColor = saturate(lighting.specularColor);

    float4 totalLightColor = ambientColor + lighting.diffuseColor + lighting.specularColor * specularColor;

    //float3 normalColor = (input.normal + float3(1, 1, 1)) * 0.5;
    //return float4(normalColor, 1.0f);
    //return float4(input.viewDirection, 1.0f);
    return textureColor * diffuseTextureTint * totalLightColor;
}
