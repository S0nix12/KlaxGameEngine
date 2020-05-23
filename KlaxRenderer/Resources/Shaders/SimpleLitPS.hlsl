/////////////
// Pixel Shader Variables
////////////

#define MAX_PER_OBJECT_LIGHTS 6
#define SHADOW_BIAS 0.0012f
#define SHADOW_BIAS_LINEAR 0.12f

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
	
	float4 positionLightSpace[MAX_PER_OBJECT_LIGHTS] : TEXCOORD3;
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
	bool bCastShadows;
	
	matrix viewProjection;
	
	int shadowMapRegister;
	
    float3 _padding0;
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

TextureCube cubeShadow0 : register(ps, t20);
TextureCube cubeShadow1 : register(ps, t21);
TextureCube cubeShadow2 : register(ps, t22);
TextureCube cubeShadow3 : register(ps, t23);
TextureCube cubeShadow4 : register(ps, t24);
TextureCube cubeShadow5 : register(ps, t25);

SamplerState shadowMapSampler : register(ps, s10);
SamplerComparisonState shadowMapComparisionSampler : register(ps, s11);

Texture2D shadowMap0 : register(ps, t26);
Texture2D shadowMap1 : register(ps, t27);
Texture2D shadowMap2 : register(ps, t28);
Texture2D shadowMap3 : register(ps, t29);
Texture2D shadowMap4 : register(ps, t30);
Texture2D shadowMap5 : register(ps, t31);


struct LightingResult
{
    float4 diffuseColor;
    float4 specularColor;
};

float vectorToDepth(float3 vec, float near, float far)
{
	float3 AbsVec = abs(vec);
	float LocalZcomp = max(AbsVec.x, max(AbsVec.y, AbsVec.z));

	float NormZComp = (far + near) / (far - near) - (2 * far * near) / (far - near) / LocalZcomp;
	return (NormZComp + 1.0) * 0.5;
}

// UE4: https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Shaders/ShadowProjectionCommon.usf
static const float2 DiscSamples5[] =
{ // 5 random points in disc with radius 2.500000
	float2(0.000000, 2.500000),
	float2(2.377641, 0.772542),
	float2(1.469463, -2.022543),
	float2(-1.469463, -2.022542),
	float2(-2.377641, 0.772543),
};

float sampleCubeShadowPCFDisc5Map0(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float sampleCubeShadowPCFDisc5Map1(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float sampleCubeShadowPCFDisc5Map2(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float sampleCubeShadowPCFDisc5Map3(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float sampleCubeShadowPCFDisc5Map4(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float sampleCubeShadowPCFDisc5Map5(float3 normalLight, float3 light, float currentDepth)
{
	float3 SideVector = normalize(cross(normalLight, float3(0, 0, 1)));
	float3 UpVector = cross(SideVector, normalLight);

	SideVector *= 1.0 / 1024.0;
	UpVector *= 1.0 / 1024.0;

	float totalShadow = 0;

	[unroll]
	for (int i = 0; i < 5; ++i)
	{
		float3 SamplePos = normalLight + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
		totalShadow += cubeShadow0.SampleCmpLevelZero(
				shadowMapComparisionSampler,
				SamplePos,
				currentDepth);
	}
	totalShadow /= 5;

	return totalShadow;
}

float CalcShadowOmni(float3 fragPos, float3 lightPos, int textureRegister, float lightRange)
{
	float3 fragToLight = fragPos - lightPos;
	float3 fragToLightN = normalize(fragToLight);
	float closestDepth = 0;
	
	float currentDepth = length(fragToLight) / lightRange - SHADOW_BIAS_LINEAR / lightRange;
	
	switch (textureRegister)
	{
		case 20:
			return cubeShadow0.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
		case 21:
			return cubeShadow1.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
		case 22:
			return cubeShadow2.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
		case 23:
			return cubeShadow3.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
		case 24:
			return cubeShadow4.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
		case 25:
			return cubeShadow5.SampleCmpLevelZero(shadowMapComparisionSampler, fragToLightN, currentDepth).r;
			break;
	}
	
	return 0.0f;
}

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

LightingResult CalcPointLightWithShadow(Light pointLight, float3 normal, float3 viewDirection, float3 positionWS)
{
	LightingResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
	result = CalcPointLight(pointLight, normal, viewDirection, positionWS);
	
	if (pointLight.bCastShadows)
	{
		float shadow = CalcShadowOmni(positionWS, pointLight.position.xyz, pointLight.shadowMapRegister, pointLight.range);
				
		result.diffuseColor *= shadow;
		result.specularColor *= shadow;
	}
	return result;
}

float CalcShadowSpot(float4 fragPosLS, int textureRegister, float lightRange)
{
	float2 shadowTexCoords;
	shadowTexCoords.x = 0.5f + (fragPosLS.x / fragPosLS.w * 0.5f);
	shadowTexCoords.y = 0.5f - (fragPosLS.y / fragPosLS.w * 0.5f);
	
	float currentDepth = fragPosLS.z / lightRange - SHADOW_BIAS_LINEAR / lightRange;
	if ((saturate(shadowTexCoords.x) == shadowTexCoords.x) &&
		(saturate(shadowTexCoords.y) == shadowTexCoords.y) &&
		currentDepth > 0)
	{
		switch (textureRegister)
		{
			case 26:
				return shadowMap0.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
			case 27:
				return shadowMap1.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
			case 28:
				return shadowMap2.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
			case 29:
				return shadowMap3.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
			case 30:
				return shadowMap4.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
			case 31:
				return shadowMap5.SampleCmpLevelZero(shadowMapComparisionSampler, shadowTexCoords, currentDepth).r;
				break;
		}
	}
	
	return 0.0f;
}

LightingResult CalcSpotLight(Light spotLight, float3 normal, float3 viewDirection, float3 positionWS, float4 positionLS)
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
	
	if (spotLight.bCastShadows)
	{
		float shadow = CalcShadowSpot(positionLS, spotLight.shadowMapRegister, spotLight.range);
				
		result.diffuseColor *= shadow;
		result.specularColor *= shadow;
	}
	
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
                lightResult = CalcPointLightWithShadow(lights[i], input.normal, viewDirection, input.positionWS.xyz);
                break;
            case SPOT_LIGHT:
                lightResult = CalcSpotLight(lights[i], input.normal, viewDirection, input.positionWS.xyz, input.positionLightSpace[i]);
                break;
        }
        lighting.diffuseColor += lightResult.diffuseColor;
        lighting.specularColor += lightResult.specularColor;
    }

    lighting.diffuseColor = saturate(lighting.diffuseColor);
    lighting.specularColor = saturate(lighting.specularColor);

	//return lighting.diffuseColor;
	float4 totalLightColor = ambientColor + lighting.diffuseColor + lighting.specularColor * specularColor;

    //float3 normalColor = (input.normal + float3(1, 1, 1)) * 0.5;
    //return float4(normalColor, 1.0f);
    //return float4(input.viewDirection, 1.0f);
    return textureColor * diffuseTextureTint * totalLightColor;
}
