﻿
struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

float4 Pixel(PSInput input) : SV_TARGET
{
    return input.color;
}